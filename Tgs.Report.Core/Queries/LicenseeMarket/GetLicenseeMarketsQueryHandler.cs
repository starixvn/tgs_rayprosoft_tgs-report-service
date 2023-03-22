using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.LicenseeMarket;

namespace Tgs.Report.Core.Queries.LicenseeMarket;

public class GetLicenseeMarketsQueryHandler : IRequestHandler<GetLicenseeMarketsRequest, LicenseeMarketsResponse>
{
    private readonly TgsContext _context;

    public GetLicenseeMarketsQueryHandler(TgsContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<LicenseeMarketsResponse> Handle(
        GetLicenseeMarketsRequest request,
        CancellationToken cancellationToken)
    {
        var offset = new TimeSpan(8, 0, 0);
        var requestFrom = request.From.ToOffset(offset);
        var requestTo = request.To.ToOffset(offset);

        var from = new DateTimeOffset(
            requestFrom.Year,
            requestFrom.Month,
            requestFrom.Day,
            0,
            0,
            0,
            requestFrom.Offset);

        var to = new DateTimeOffset(
            requestTo.Year,
            requestTo.Month,
            requestTo.Day,
            0,
            0,
            0,
            requestTo.Offset);

        var now = DateTimeOffset.Now.ToOffset(offset);

        var yesterday = new DateTimeOffset(now.Year,
            now.Month,
            now.Day,
            0,
            0,
            0,
            requestTo.Offset).AddDays(-1);

        var fromDate = new DateOnly(from.Year, from.Month, from.Day);
        var toDate = to > yesterday
            ? new(yesterday.Year, yesterday.Month, yesterday.Day)
            : new DateOnly(to.Year, to.Month, to.Day);

        var brands = await _context.Brands
            .Where(x => x.LicenseeId == request.LicenseeId)
            .Where(!string.IsNullOrEmpty(request.CurrencyCode),
                x => x.BrandCurrencies.Any(c => c.CurrencyCode == request.CurrencyCode))
            .Where(request.CurrencyCodes != null,
                x => request.CurrencyCodes!.Contains(x.BrandCurrencies.FirstOrDefault()!.CurrencyCode))
            .Select(x => new
            {
                x.LicenseeId,
                BrandId = x.Id,
                BrandCode = x.Code,
                CurrencyCode = string.Join(',', x.BrandCurrencies.Select(c => c.CurrencyCode))
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var brandIds = brands.Select(x => x.BrandId).ToArray();

        var dailyRealTimeWithdraws = to > yesterday
            ? await _context.WithdrawForms
                .Where(x => brandIds.Contains(x.BrandId))
                .Where(x => x.UserType == 1 && x.Status == 9000)
                .Where(from > yesterday, x => x.PaidOn >= from.UtcDateTime && x.PaidOn < to.UtcDateTime)
                .Where(!(from > yesterday), x => x.PaidOn >= yesterday.UtcDateTime && x.PaidOn < to.UtcDateTime)
                .GroupBy(x => x.BrandId)
                .Select(withdrawal => new
                {
                    BrandId = withdrawal.Key,
                    WithdrawalAmount = withdrawal.Sum(w => w.ActualAmount ?? w.Amount)
                })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false)
            : null;

        var dailyRealTimeDeposits = to > yesterday
            ? await _context.DepositForms
                .Where(x => brandIds.Contains(x.BrandId))
                .Where(x => x.Status == 8000)
                .Where(from > yesterday, x => x.UpdatedOn >= from.UtcDateTime && x.UpdatedOn < to.UtcDateTime)
                .Where(!(from > yesterday), x => x.UpdatedOn >= yesterday.UtcDateTime && x.UpdatedOn < to.UtcDateTime)
                .GroupBy(x => x.BrandId)
                .Select(deposit => new
                {
                    BrandId = deposit.Key,
                    DepositAmount = deposit.Sum(d => d.ActualAmount ?? d.Amount),
                    DepositHandlingFee = deposit.Sum(d => d.HandlingFee)
                })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false)
            : null;

        var brandPaymentStatistics = await _context.BrandDailyPaymentStatistics
            .Where(x => x.ReportDate >= fromDate && x.ReportDate < toDate)
            .GroupBy(x => x.BrandId,
                (brandId, statistics) => new
                {
                    BrandId = brandId,
                    DailyDepositAmount = statistics.Sum(x => x.DailyDepositAmount),
                    DailyDepositHandlingFee = statistics.Sum(x => x.DailyDepositHandlingFee),
                    DailyWithdrawAmount = statistics.Sum(x => x.DailyWithdrawAmount)
                })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var marketAggregates = await _context.Brands
            .GroupJoin(_context.PlayerAggregates.Where(x => x.ReportDate >= from.UtcDateTime &&
                    x.ReportDate < to.UtcDateTime),
                brand => brand.Id,
                aggregate => aggregate.BrandId,
                (brand, aggregate) => new
                {
                    Aggregate = aggregate,
                    BrandId = brand.Id,
                    BrandCode = brand.Code,
                    brand.LicenseeId
                })
            .SelectMany(x => x.Aggregate.DefaultIfEmpty(),
                (x, aggregate) => new
                {
                    x.LicenseeId,
                    x.BrandId,
                    x.BrandCode,
                    AgentAggregate = aggregate
                })
            .Where(x => x.LicenseeId == request.LicenseeId)
            .GroupJoin(_context.BrandCurrencies,
                agent => agent.BrandId,
                brandCurrency => brandCurrency.BrandId,
                (agent, currencies) => new
                {
                    agent.AgentAggregate,
                    agent.BrandId,
                    agent.BrandCode,
                    BrandCurrencies = currencies
                })
            .SelectMany(x => x.BrandCurrencies.DefaultIfEmpty(),
                (x, brandCurrency) => new
                {
                    x.AgentAggregate,
                    x.BrandId,
                    x.BrandCode,
                    BrandCurrencyCode = brandCurrency.CurrencyCode
                })
            .Where(!string.IsNullOrEmpty(request.CurrencyCode),
                x => x.BrandCurrencyCode == request.CurrencyCode)
            .Where(request.CurrencyCodes != null,
                x => request.CurrencyCodes!.Contains(x.BrandCurrencyCode))
            .GroupJoin(_context.PlayerBonusAggregates,
                agent => new
                {
                    agent.AgentAggregate.ReportDate,
                    agent.AgentAggregate.Hour,
                    agent.AgentAggregate.AgentId,
                    agent.AgentAggregate.PlayerCode
                },
                bonus => new
                {
                    bonus.ReportDate,
                    bonus.Hour,
                    bonus.AgentId,
                    bonus.PlayerCode
                },
                (agent, bonus) => new
                {
                    agent.AgentAggregate,
                    agent.BrandId,
                    agent.BrandCode,
                    agent.BrandCurrencyCode,
                    BonusAggregates = bonus
                })
            .SelectMany(x => x.BonusAggregates.DefaultIfEmpty(),
                (x, bonusAggregate) => new
                {
                    x.AgentAggregate,
                    x.BrandId,
                    x.BrandCode,
                    x.BrandCurrencyCode,
                    BonusAggregate = bonusAggregate
                })
            .GroupBy(x => new
            {
                x.BrandId,
                x.BrandCode,
                x.BrandCurrencyCode
            }, (x, aggregates) => new
            {
                x.BrandId,
                x.BrandCode,
                x.BrandCurrencyCode,
                CompanyAdjustAmount =
                    aggregates.Sum(s
                        => s.AgentAggregate.CompanyAdjustAmount -
                        s.AgentAggregate.NoIncludeNetProfitCompanyAdjustAmount),
                BonusAmount =
                    aggregates.Sum(s
                        => s.AgentAggregate.RebateAmount + s.AgentAggregate.PromotionAmount + (s.BonusAggregate == null
                            ? 0M
                            : s.BonusAggregate.UpgradeAmount + s.BonusAggregate.SeasonAmount +
                            s.BonusAggregate.BirthdayAmount)),
                BetAmount = aggregates.Sum(s => s.AgentAggregate.BetAmount),
                Turnover = aggregates.Sum(s => s.AgentAggregate.Turnover),
                WinLossAmount = aggregates.Sum(s => s.AgentAggregate.WinLossAmount),
                TotalAgentDividend = aggregates.Sum(m
                    => m.AgentAggregate.AgentId == null
                        ? 0M
                        : (m.AgentAggregate.WinLossAmount - (m.AgentAggregate.RebateAmount +
                            m.AgentAggregate.PromotionAmount +
                            (m.BonusAggregate == null
                                ? 0M
                                : m.BonusAggregate.UpgradeAmount + m.BonusAggregate.SeasonAmount +
                                m.BonusAggregate.BirthdayAmount) +
                            m.AgentAggregate.BonusAdjustAmount -
                            m.AgentAggregate.NoIncludeNetProfitBonusAdjustAmount)) *
                        m.AgentAggregate.ChiefAgentDividendPercentage * 0.01M),
                NetProfit = aggregates.Sum(m
                    => m.AgentAggregate.WinLossAmount - (m.AgentAggregate.CompanyAdjustAmount -
                        m.AgentAggregate.NoIncludeNetProfitCompanyAdjustAmount) -
                    ((m.AgentAggregate.RebateAmount + m.AgentAggregate.PromotionAmount +
                            (m.BonusAggregate == null
                                ? 0M
                                : m.BonusAggregate.UpgradeAmount + m.BonusAggregate.SeasonAmount +
                                m.BonusAggregate.BirthdayAmount) +
                            m.AgentAggregate.BonusAdjustAmount - m.AgentAggregate.NoIncludeNetProfitBonusAdjustAmount) *
                        (m.AgentAggregate.AgentId == null
                            ? 1
                            : (100 - m.AgentAggregate.ChiefAgentDividendPercentage) * 0.01M) -
                        (m.AgentAggregate.AgentId == null
                            ? 0M
                            : (m.AgentAggregate.WinLossAmount - (m.AgentAggregate.RebateAmount +
                                m.AgentAggregate.PromotionAmount +
                                (m.BonusAggregate == null
                                    ? 0M
                                    : m.BonusAggregate.UpgradeAmount + m.BonusAggregate.SeasonAmount +
                                    m.BonusAggregate.BirthdayAmount) +
                                m.AgentAggregate.BonusAdjustAmount -
                                m.AgentAggregate.NoIncludeNetProfitBonusAdjustAmount)) *
                            m.AgentAggregate.ChiefAgentDividendPercentage * 0.01M)))
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var groupData = marketAggregates
            .Select(x =>
            {
                var paymentStatistics = brandPaymentStatistics
                    .FirstOrDefault(s => s.BrandId == x.BrandId);

                var realTimeDeposit = dailyRealTimeDeposits?
                    .FirstOrDefault(s => s.BrandId == x.BrandId);

                var realTimeWithdraw = dailyRealTimeWithdraws?
                    .FirstOrDefault(s => s.BrandId == x.BrandId);

                return new LicenseeMarketDataModel
                {
                    BrandId = x.BrandId,
                    BrandCode = x.BrandCode,
                    CurrencyCode = x.BrandCurrencyCode,
                    DepositAmount = (paymentStatistics?.DailyDepositAmount ?? 0M)
                        + (realTimeDeposit?.DepositAmount ?? 0M),
                    DepositHandlingFee = (paymentStatistics?.DailyDepositHandlingFee ?? 0M)
                        + (realTimeDeposit?.DepositHandlingFee ?? 0M),
                    WithdrawalAmount = decimal.Negate(
                        (paymentStatistics?.DailyWithdrawAmount ?? 0M)
                        + (realTimeWithdraw?.WithdrawalAmount ?? 0M)),
                    CompanyAdjustAmount = x.CompanyAdjustAmount,
                    BonusAmount = x.BonusAmount,
                    BetAmount = x.BetAmount,
                    Turnover = x.Turnover,
                    WinLossAmount = x.WinLossAmount,
                    NetProfit = x.NetProfit ?? 0M,
                    AgentProfit = x.TotalAgentDividend ?? 0M
                };
            });

        var total = groupData
            .GroupBy(c => c.CurrencyCode)
            .Select(currency => new LicenseeMarketTotalModel
            {
                CurrencyCode = currency.Key,
                DepositAmount = currency.Sum(x => x.DepositAmount),
                DepositHandlingFee = currency.Sum(x => x.DepositHandlingFee),
                WithdrawalAmount = currency.Sum(x => x.WithdrawalAmount),
                CompanyAdjustAmount = currency.Sum(x => x.CompanyAdjustAmount),
                BonusAmount = currency.Sum(x => x.BonusAmount),
                BetAmount = currency.Sum(x => x.BetAmount),
                Turnover = currency.Sum(x => x.Turnover),
                WinLossAmount = currency.Sum(x => x.WinLossAmount),
                NetProfit = currency.Sum(x => x.NetProfit),
                AgentProfit = currency.Sum(x => x.AgentProfit)
            })
            .ToArray();

        var datas = groupData.AsQueryable().CreatePagedResponse(request);

        return new()
        {
            Datas = datas,
            Totals = total
        };
    }
}