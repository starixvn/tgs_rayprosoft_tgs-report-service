using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.ResellerMarket;

namespace Tgs.Report.Core.Queries.ResellerMarket;

public class GetResellerMarketsQueryHandler : IRequestHandler<GetResellerMarketsRequest, ResellerMarketsResponse>
{
    private readonly ITgsService<Brand> _brandService;
    private readonly TgsContext _context;
    private readonly ITgsService<PlayerAggregate> _playerAggregateService;
    private readonly ITgsService<PlayerBonusAggregate> _playerBonusAggregateService;
    private readonly ITgsService<WithdrawForm> _withdrawFormService;

    public GetResellerMarketsQueryHandler(
        TgsContext context,
        ITgsService<Brand> brandService,
        ITgsService<WithdrawForm> withdrawFormService,
        ITgsService<PlayerAggregate> playerAggregateService,
        ITgsService<PlayerBonusAggregate> playerBonusAggregateService)
    {
        _context = context;
        _brandService = brandService;
        _withdrawFormService = withdrawFormService;
        _playerAggregateService = playerAggregateService;
        _playerBonusAggregateService = playerBonusAggregateService;
    }

    public async Task<ResellerMarketsResponse> Handle(
        GetResellerMarketsRequest request,
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
            ? new DateOnly(yesterday.Year, yesterday.Month, yesterday.Day)
            : new DateOnly(to.Year, to.Month, to.Day);

        var brands = await _brandService.GetQueries()
            .Where(x => x.Licensee.ResellerId == request.ResellerId)
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

        var brandIds = brands.Select(b => b.BrandId).ToArray();

        var dailyRealTimeWithdrawals = to > yesterday
            ? await _withdrawFormService.GetQueries()
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
                    DailyDepositAmount = statistics.Sum(s => s.DailyDepositAmount),
                    DailyDepositHandlingFee = statistics.Sum(s => s.DailyDepositHandlingFee),
                    DailyWithdrawAmount = statistics.Sum(s => s.DailyWithdrawAmount)
                })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var playerBonuses = await _playerBonusAggregateService.GetQueries()
            .Where(x => brandIds.Contains(x.BrandId))
            .Where(x => x.ReportDate >= from.ToUniversalTime()
                        && x.ReportDate < to.ToUniversalTime())
            .GroupBy(x => x.BrandId,
                (brandId, aggregates) => new ResellerMarketModel
                {
                    BrandId = brandId,
                    RebateAmount = aggregates.Sum(s => s.RebateAmount),
                    PromotionAmount = aggregates.Sum(s => s.PromotionAmount),
                    UpgradeAmount = aggregates.Sum(s => s.UpgradeAmount),
                    SeasonAmount = aggregates.Sum(s => s.SeasonAmount),
                    BirthdayAmount = aggregates.Sum(s => s.BirthdayAmount),
                    BonusAmount = aggregates.Sum(m => m.UpgradeAmount + m.SeasonAmount + m.BirthdayAmount)
                })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var playerAggregates = await _playerAggregateService.GetQueries()
            .Where(x => brandIds.Contains(x.BrandId))
            .Where(x => x.ReportDate >= from.ToUniversalTime()
                        && x.ReportDate < to.ToUniversalTime())
            .GroupBy(x => x.BrandId)
            .Select(playerAggregate => new ResellerMarketModel
            {
                BrandId = playerAggregate.Key,
                CompanyAdjustAmount =
                    playerAggregate.Sum(m => m.CompanyAdjustAmount - m.NoIncludeNetProfitCompanyAdjustAmount),
                BonusAmount = playerAggregate.Sum(m => m.RebateAmount + m.PromotionAmount),
                BetAmount = playerAggregate.Sum(m => m.BetAmount),
                Turnover = playerAggregate.Sum(m => m.Turnover),
                WinLossAmount = playerAggregate.Sum(m => m.WinLossAmount),
                NetProfit = playerAggregate.Sum(m
                    => m.WinLossAmount
                       - m.CompanyAdjustAmount
                       - m.RebateAmount
                       - m.PromotionAmount)
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var groupDatas = brands
            .GroupJoin(
                playerAggregates.Concat(playerBonuses),
                brand => brand.BrandId,
                playerAggregate => playerAggregate.BrandId,
                (brand, aggregates) => new
                {
                    Brand = brand,
                    PlayerAggregates = aggregates
                })
            .SelectMany(
                x => x.PlayerAggregates.DefaultIfEmpty(),
                (x, aggregate) => new
                {
                    x.Brand.LicenseeId,
                    x.Brand.BrandId,
                    x.Brand.BrandCode,
                    x.Brand.CurrencyCode,
                    PlayerAggregate = aggregate
                })
            .GroupBy(
                x => new
                {
                    x.LicenseeId,
                    x.BrandId,
                    x.BrandCode,
                    x.CurrencyCode
                },
                (key, aggregates) =>
                {
                    var paymentStatistics = brandPaymentStatistics
                        .FirstOrDefault(ps => ps.BrandId == key.BrandId);

                    var realTimeDeposit = dailyRealTimeDeposits?
                        .FirstOrDefault(x => x.BrandId == key.BrandId);

                    var realTimeWithdraw = dailyRealTimeWithdrawals?
                        .FirstOrDefault(w => w.BrandId == key.BrandId);

                    return new ResellerMarketDataModel
                    {
                        BrandId = key.BrandId,
                        BrandCode = key.BrandCode,
                        CurrencyCode = key.CurrencyCode,
                        DepositAmount = (paymentStatistics?.DailyDepositAmount ?? 0M)
                                        + (realTimeDeposit?.DepositAmount ?? 0M),
                        DepositHandlingFee = (paymentStatistics?.DailyDepositHandlingFee ?? 0M)
                                             + (realTimeDeposit?.DepositHandlingFee ?? 0M),
                        WithdrawalAmount = decimal.Negate((paymentStatistics?.DailyWithdrawAmount ?? 0M)
                                                          + (realTimeWithdraw?.WithdrawalAmount ?? 0M)),
                        CompanyAdjustAmount = aggregates.Sum(m => m.PlayerAggregate?.CompanyAdjustAmount ?? 0M),
                        BonusAmount = aggregates.Sum(m => m.PlayerAggregate?.BonusAmount ?? 0M),
                        BetAmount = aggregates.Sum(m => m.PlayerAggregate?.BetAmount ?? 0M),
                        Turnover = aggregates.Sum(m => m.PlayerAggregate?.Turnover ?? 0M),
                        WinLossAmount = aggregates.Sum(m => m.PlayerAggregate?.WinLossAmount ?? 0M),
                        NetProfit = aggregates.Sum(m => m.PlayerAggregate == null
                                                            ? 0M
                                                            : m.PlayerAggregate.NetProfit
                                                              - m.PlayerAggregate.UpgradeAmount
                                                              - m.PlayerAggregate.SeasonAmount
                                                              - m.PlayerAggregate.BirthdayAmount)
                    };
                });

        var total = groupDatas
            .GroupBy(c => c.CurrencyCode)
            .Select(currency => new ResellerMarketTotalModel
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
                NetProfit = currency.Sum(x => x.NetProfit)
            })
            .ToArray();

        var datas = groupDatas.AsQueryable().CreatePagedResponse(request);

        return new ResellerMarketsResponse
        {
            Datas = datas,
            Totals = total
        };
    }
}