using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Market;

namespace Tgs.Report.Core.Queries.Market;

public class GetMarketsQueryHandler : IRequestHandler<GetMarketsRequest, PagedResponse<MarketModel>>
{
    private readonly TgsContext _context;

    public GetMarketsQueryHandler(TgsContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<PagedResponse<MarketModel>> Handle(
        GetMarketsRequest request,
        CancellationToken cancellationToken)
    {
        var offset = new TimeSpan(8, 0, 0);
        var requestFrom = request.From.ToOffset(offset);
        var requestTo = request.To.ToOffset(offset);

        var from = new DateTimeOffset(requestFrom.Year,
            requestFrom.Month,
            requestFrom.Day,
            0,
            0,
            0,
            requestFrom.Offset);

        var to = new DateTimeOffset(requestTo.Year,
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

        var dates = Enumerable.Range(0, to.Subtract(from).Days)
            .Select(x => from.AddDays(x))
            .ToArray();

        var withdrawals = to > yesterday
            ? await _context.WithdrawForms
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.UserType == 1 && x.Status == 9000)
                .Where(x => x.PaidOn >= yesterday.ToUniversalTime() && x.PaidOn < to.ToUniversalTime())
                .Select(x => new
                {
                    x.PaidOn,
                    x.ActualAmount,
                    x.Amount
                })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false)
            : null;

        var withdrawalByDates = withdrawals?.GroupBy(x => x.PaidOn!.Value.ToOffset(offset).Date,
            (date, withdrawals) => new
            {
                Date = date,
                WithdrawalAmount = withdrawals.Sum(w => w.ActualAmount ?? w.Amount)
            });

        var deposits = to > yesterday
            ? await _context.DepositForms
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.Status == 8000)
                .Where(x => x.UpdatedOn >= yesterday.ToUniversalTime()
                    && x.UpdatedOn < to.ToUniversalTime())
                .Select(x => new
                {
                    x.UpdatedOn,
                    x.ActualAmount,
                    x.Amount,
                    x.HandlingFee
                })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false)
            : null;

        var depositByDates = deposits?
            .GroupBy(x => x.UpdatedOn.ToOffset(offset).Date,
                (date, deposits) => new
                {
                    Date = date,
                    DepositAmount = deposits.Sum(d => d.ActualAmount ?? d.Amount),
                    DepositHandlingFee = deposits.Sum(d => d.HandlingFee)
                });

        var playerLoginRecordsByDates = await _context.PlayerLoginRecords
            .Where(plr => plr.BrandId == request.BrandId)
            .Where(plr => plr.CreatedOn >= from.ToUniversalTime() && plr.CreatedOn < to.ToUniversalTime())
            .Select(plr => new
            {
                plr.CreatedOn.AddHours(8).Date,
                plr.PlayerCode
            })
            .Distinct()
            .GroupBy(x => x.Date,
                (date, playerLoginRecords) => new
                {
                    Date = date,
                    LoginPlayer = playerLoginRecords.Sum(plr => 1)
                })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var fromDate = new DateOnly(from.Year, from.Month, from.Day);
        var toDate = new DateOnly(to.Year, to.Month, to.Day);

        var brandPaymentStatistics = await _context.BrandDailyPaymentStatistics
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate >= fromDate && x.ReportDate < toDate)
            .Select(x => new
            {
                x.ReportDate,
                x.DailyDepositAmount,
                x.DailyDepositHandlingFee,
                x.DailyWithdrawAmount
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var marketAggregates = await _context.PlayerAggregates
            .GroupJoin(_context.PlayerBonusAggregates,
                agent => new
                {
                    agent.ReportDate,
                    agent.Hour,
                    agent.AgentId,
                    agent.PlayerCode
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
                    AgentAggregate = agent,
                    BonusAggregates = bonus
                })
            .SelectMany(x => x.BonusAggregates.DefaultIfEmpty(),
                (x, bonusAggregate) => new
                {
                    x.AgentAggregate,
                    BonusAggregate = bonusAggregate
                })
            .Where(x => x.AgentAggregate.BrandId == request.BrandId)
            .Where(x => x.AgentAggregate.ReportDate >= from.ToUniversalTime() &&
                x.AgentAggregate.ReportDate < to.ToUniversalTime())
            .GroupBy(x => x.AgentAggregate.ReportDate, (x, aggregates) => new
            {
                ReportDate = x,
                DepositCount =
                    aggregates.Where(s => s.AgentAggregate.DepositAmount > 0).Select(s => s.AgentAggregate.PlayerCode)
                        .Distinct().Count(),
                ActivePlayer =
                    aggregates.Where(s => s.AgentAggregate.BetAmount > 0).Select(s => s.AgentAggregate.PlayerCode)
                        .Distinct().Count(),
                RegisteredCount = aggregates.Sum(s => s.AgentAggregate.RegisteredCount),
                CompanyAdjustAmount = aggregates.Sum(s => s.AgentAggregate.CompanyAdjustAmount),
                NoIncludeNetProfitCompanyAdjustAmount =
                    aggregates.Sum(s => s.AgentAggregate.NoIncludeNetProfitCompanyAdjustAmount),
                BonusAdjustAmount = aggregates.Sum(s => s.AgentAggregate.BonusAdjustAmount),
                NoIncludeNetProfitBonusAdjustAmount =
                    aggregates.Sum(s => s.AgentAggregate.NoIncludeNetProfitBonusAdjustAmount),
                RebateAmount = aggregates.Sum(s => s.AgentAggregate.RebateAmount),
                PromotionAmount = aggregates.Sum(s => s.AgentAggregate.PromotionAmount),
                BetAmount = aggregates.Sum(s => s.AgentAggregate.BetAmount),
                OriginalTurnover = aggregates.Sum(s => s.AgentAggregate.OriginalTurnover),
                Turnover = aggregates.Sum(s => s.AgentAggregate.Turnover),
                WinLossAmount = aggregates.Sum(s => s.AgentAggregate.WinLossAmount),
                AllAgentRebateAmount = aggregates.Sum(s => s.AgentAggregate.AllAgentRebateAmount),
                FirstFundInCount = aggregates.Sum(s => s.AgentAggregate.FirstFundInCount),
                UpgradeAmount = aggregates.Sum(s => s.BonusAggregate == null ? 0M : s.BonusAggregate.UpgradeAmount),
                SeasonAmount = aggregates.Sum(s => s.BonusAggregate == null ? 0M : s.BonusAggregate.SeasonAmount),
                BirthdayAmount = aggregates.Sum(s => s.BonusAggregate == null ? 0M : s.BonusAggregate.BirthdayAmount),
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
                        (m.AgentAggregate.ChiefAgentDividendPercentage / 100)).ToString(),
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
                            : (100 - m.AgentAggregate.ChiefAgentDividendPercentage) / 100) -
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
                            (m.AgentAggregate.ChiefAgentDividendPercentage / 100)))).ToString(),
                PlatformCoveredBonus =
                    aggregates.Sum(
                        m => m.AgentAggregate.ChiefAgentDividendPercentage == null
                            ? 0M
                            : (m.AgentAggregate.RebateAmount + m.AgentAggregate.PromotionAmount +
                                (m.BonusAggregate == null
                                    ? 0M
                                    : m.BonusAggregate.UpgradeAmount + m.BonusAggregate.SeasonAmount +
                                    m.BonusAggregate.BirthdayAmount)) *
                            ((100 - m.AgentAggregate.ChiefAgentDividendPercentage.Value) / 100)).ToString(),
                PlatformCoveredBonusAdjust =
                    aggregates.Sum(m => m.AgentAggregate.ChiefAgentDividendPercentage == null
                        ? 0M
                        : (m.AgentAggregate.BonusAdjustAmount - m.AgentAggregate.NoIncludeNetProfitBonusAdjustAmount) *
                        ((100 - m.AgentAggregate.ChiefAgentDividendPercentage.Value) / 100)).ToString()
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return dates.GroupJoin(marketAggregates,
                date => date,
                market => market.ReportDate,
                (date, markets) => new
                {
                    ReportDate = date,
                    MarketAggregates = markets
                })
            .SelectMany(x => x.MarketAggregates.DefaultIfEmpty(),
                (x, market) => new
                {
                    x.ReportDate,
                    MarketAggregate = market
                })
            .GroupJoin(brandPaymentStatistics,
                market => new(market.ReportDate.Year, market.ReportDate.Month, market.ReportDate.Day),
                deposit => deposit.ReportDate,
                (group, paymentStatistics) => new
                {
                    group.ReportDate,
                    group.MarketAggregate,
                    PaymentStatistics = paymentStatistics
                })
            .SelectMany(x => x.PaymentStatistics.DefaultIfEmpty(),
                (x, paymentStatistic) => new
                {
                    x.ReportDate,
                    x.MarketAggregate,
                    PaymentStatistic = paymentStatistic
                })
            .GroupBy(x => x.ReportDate,
                (reportDate, aggregates) =>
                {
                    var realTimeDeposit = depositByDates?.FirstOrDefault(d => d.Date == reportDate.Date);
                    var realTimeWithdraw = withdrawalByDates?.FirstOrDefault(d => d.Date == reportDate.Date);

                    return new MarketModel
                    {
                        ReportDate = reportDate,
                        DepositCount = aggregates.Sum(m => m.MarketAggregate?.DepositCount ?? 0),
                        ActivePlayer = aggregates.Sum(m => m.MarketAggregate?.ActivePlayer ?? 0),
                        LoginPlayer =
                            playerLoginRecordsByDates.FirstOrDefault(plr => plr.Date == reportDate.Date)
                                ?.LoginPlayer ?? 0,
                        RegisteredCount = aggregates.Sum(m => m.MarketAggregate?.RegisteredCount ?? 0),
                        FirstFundInCount = aggregates.Sum(m => m.MarketAggregate?.FirstFundInCount ?? 0),
                        DepositAmount =
                            realTimeDeposit?.DepositAmount ??
                            aggregates.Sum(m => m.PaymentStatistic?.DailyDepositAmount ?? 0M),
                        DepositHandlingFee =
                            realTimeDeposit?.DepositHandlingFee ??
                            aggregates.Sum(m => m.PaymentStatistic?.DailyDepositHandlingFee ?? 0M),
                        WithdrawalAmount =
                            realTimeWithdraw?.WithdrawalAmount ??
                            aggregates.Sum(m => m.PaymentStatistic?.DailyWithdrawAmount ?? 0M),
                        CompanyAdjustAmount = aggregates.Sum(m
                            => m.MarketAggregate == null
                                ? 0M
                                : m.MarketAggregate.CompanyAdjustAmount -
                                m.MarketAggregate.NoIncludeNetProfitCompanyAdjustAmount),
                        BonusAdjustAmount = aggregates.Sum(m
                            => m.MarketAggregate == null
                                ? 0M
                                : m.MarketAggregate.BonusAdjustAmount -
                                m.MarketAggregate.NoIncludeNetProfitBonusAdjustAmount),
                        BonusAmount =
                            aggregates.Sum(
                                m => m.MarketAggregate == null
                                    ? 0
                                    : m.MarketAggregate.RebateAmount
                                    + m.MarketAggregate.PromotionAmount
                                    + m.MarketAggregate.UpgradeAmount
                                    + m.MarketAggregate.SeasonAmount
                                    + m.MarketAggregate.BirthdayAmount),
                        BetAmount = aggregates.Sum(m => m.MarketAggregate?.BetAmount ?? 0M),
                        ValidBetAmount = aggregates.Sum(m => m.MarketAggregate?.OriginalTurnover ?? 0M),
                        Turnover = aggregates.Sum(m => m.MarketAggregate?.Turnover ?? 0M),
                        WinLossAmount = aggregates.Sum(m => m.MarketAggregate?.WinLossAmount ?? 0M),
                        NetProfit = aggregates.Sum(m => decimal.Parse(m.MarketAggregate?.NetProfit ?? "0")),
                        AdditionalExpenses = aggregates.Sum(m => m.MarketAggregate?.AllAgentRebateAmount ?? 0M),
                        PlatformCoveredBonus =
                            aggregates.Sum(m => decimal.Parse(m.MarketAggregate?.PlatformCoveredBonus ?? "0")),
                        PlatformCoveredBonusAdjust =
                            aggregates.Sum(m => decimal.Parse(m.MarketAggregate?.PlatformCoveredBonusAdjust ?? "0")),
                        AgentProfit = aggregates.Sum(m => decimal.Parse(m.MarketAggregate?.TotalAgentDividend ?? "0"))
                    };
                })
            .AsQueryable()
            .CreatePagedResponse(request);
    }
}