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

    public GetResellerMarketsQueryHandler(TgsContext context, ITgsService<Brand> brandService,
        ITgsService<WithdrawForm> withdrawFormService, ITgsService<PlayerAggregate> playerAggregateService,
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

        var today = new DateTimeOffset(now.Year,
            now.Month,
            now.Day,
            0,
            0,
            0,
            requestTo.Offset);

        var fromDate = new DateOnly(from.Year, from.Month, from.Day);
        var toDate = new DateOnly(to.Year, to.Month, to.Day);

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
                CurrencyCode = string.Join(',', x.BrandCurrencies.Select(x => x.CurrencyCode).ToArray())
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var withdrawals = to > today
            ? await _withdrawFormService.GetQueries()
                .Where(x => brands.Select(x => x.BrandId).Contains(x.BrandId))
                .Where(x => x.UserType == 1 && x.Status == 9000)
                .Where(x => x.CreatedOn >= today.ToUniversalTime() && x.CreatedOn < today.AddDays(1).ToUniversalTime())
                .GroupBy(x => x.BrandId)
                .Select(withdrawal => new
                {
                    BrandId = withdrawal.Key,
                    WithdrawalAmount = -withdrawal.Sum(w => w.ActualAmount ?? w.Amount)
                })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false)
            : null;

        var deposits = to > today
            ? await _context.DepositForms
                .Where(x => brands.Select(x => x.BrandId).Contains(x.BrandId))
                .Where(x => x.Status == 8000)
                .Where(x => x.CreatedOn >= today.ToUniversalTime() && x.CreatedOn < today.AddDays(1).ToUniversalTime())
                .GroupBy(x => x.BrandId)
                .Select(deposit => new
                {
                    BrandId = deposit.Key,
                    DepositAmount = deposit.Sum(w => w.ActualAmount ?? w.Amount)
                })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false)
            : null;

        var dailyDepositWithdraws = await _context.PlayerDailyPaymentStatistics
            .Join(_context.Players.Where(x => brands.Select(x => x.BrandId).Contains(x.BrandId)), pp => pp.PlayerCode,
                p => p.PlayerCode, (pp, p) => new
                {
                    pp.ReportDate,
                    p.BrandId,
                    pp.DailyDepositAmount,
                    pp.DailyWithdrawAmount
                })
            .Where(x => x.ReportDate >= fromDate && x.ReportDate < toDate)
            .GroupBy(x => x.BrandId, (brandId, group) => new
            {
                BrandId = brandId,
                DailyDepositAmount = group.Sum(s => s.DailyDepositAmount),
                DailyWithdrawAmount = group.Sum(s => s.DailyWithdrawAmount)
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var playerBonuses = await _playerBonusAggregateService.GetQueries()
            .Where(x => brands.Select(x => x.BrandId).Contains(x.BrandId))
            .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
            .GroupBy(x => x.BrandId, (brandId, group) => new ResellerMarketModel
            {
                BrandId = brandId,
                RebateAmount = group.Sum(s => s.RebateAmount),
                PromotionAmount = group.Sum(s => s.PromotionAmount),
                UpgradeAmount = group.Sum(s => s.UpgradeAmount),
                SeasonAmount = group.Sum(s => s.SeasonAmount),
                BirthdayAmount = group.Sum(s => s.BirthdayAmount),
                BonusAmount = group.Sum(m => m.UpgradeAmount + m.SeasonAmount + m.BirthdayAmount)
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var playerAggregates = await _playerAggregateService.GetQueries()
            .Where(x => brands.Select(x => x.BrandId).Contains(x.BrandId))
            .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
            .GroupBy(x => x.BrandId)
            .Select(playerAggregate => new ResellerMarketModel
            {
                BrandId = playerAggregate.Key,
                RegisteredCount = playerAggregate.Sum(m => m.RegisteredCount),
                FirstFundInCount = playerAggregate.Sum(m => m.FirstFundInCount),
                CompanyAdjustAmount =
                    playerAggregate.Sum(m => m.CompanyAdjustAmount - m.NoIncludeNetProfitCompanyAdjustAmount),
                BonusAmount = playerAggregate.Sum(m => m.RebateAmount + m.PromotionAmount),
                BetAmount = playerAggregate.Sum(m => m.BetAmount),
                Turnover = playerAggregate.Sum(m => m.Turnover),
                WinLossAmount = playerAggregate.Sum(m => m.WinLossAmount),
                NetProfit = playerAggregate.Sum(m
                    => m.WinLossAmount - m.CompanyAdjustAmount - m.RebateAmount - m.PromotionAmount)
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var groupDatas = brands
            .GroupJoin(
                playerAggregates.Concat(playerBonuses),
                brand => brand.BrandId,
                playerAggregate => playerAggregate.BrandId,
                (brand, playerAggregate) => new
                {
                    brand,
                    playerAggregate
                })
            .SelectMany(
                x => x.playerAggregate.DefaultIfEmpty(),
                (x, market) => new
                {
                    x.brand.LicenseeId,
                    x.brand.BrandId,
                    x.brand.BrandCode,
                    x.brand.CurrencyCode,
                    market
                })
            .GroupBy(
                x => new
                {
                    x.LicenseeId,
                    x.BrandId,
                    x.BrandCode,
                    x.CurrencyCode
                },
                (market, playerAggregates) =>
                    new ResellerMarketDataModel
                    {
                        BrandId = market.BrandId,
                        BrandCode = market.BrandCode,
                        CurrencyCode = market.CurrencyCode,
                        RegisteredCount = playerAggregates.Sum(m => m.market?.RegisteredCount ?? 0),
                        FirstFundInCount = playerAggregates.Sum(m => m.market?.FirstFundInCount ?? 0),
                        DepositAmount = dailyDepositWithdraws.Where(x => x.BrandId == market.BrandId).Sum(m => m.DailyDepositAmount ?? 0M)
                            + (deposits?.FirstOrDefault(w => w.BrandId == market.BrandId)
                                ?.DepositAmount ?? 0M),
                        WithdrawalAmount =
                            -(dailyDepositWithdraws.Where(x => x.BrandId == market.BrandId).Sum(m => m.DailyWithdrawAmount ?? 0M)
                                + (withdrawals?.FirstOrDefault(w => w.BrandId == market.BrandId)
                                    ?.WithdrawalAmount ?? 0M)),
                        CompanyAdjustAmount = playerAggregates.Sum(m => m.market?.CompanyAdjustAmount ?? 0),
                        BonusAmount = playerAggregates.Sum(m => m.market?.BonusAmount ?? 0),
                        BetAmount = playerAggregates.Sum(m => m.market?.BetAmount ?? 0),
                        Turnover = playerAggregates.Sum(m => m.market?.Turnover ?? 0),
                        WinLossAmount = playerAggregates.Sum(m => m.market?.WinLossAmount ?? 0),
                        NetProfit = playerAggregates.Sum(m => (m.market?.NetProfit ?? 0) -
                            (m.market == null
                                ? 0
                                : m.market.UpgradeAmount + m.market.SeasonAmount + m.market.BirthdayAmount))
                    });

        var total = groupDatas
            .GroupBy(c => c.CurrencyCode)
            .Select(Currency => new ResellerMarketTotalModel
            {
                CurrencyCode = Currency.Key,
                RegisteredCount = Currency.Sum(x => x.RegisteredCount),
                FirstFundInCount = Currency.Sum(x => x.FirstFundInCount),
                DepositAmount = Currency.Sum(x => x.DepositAmount),
                WithdrawalAmount = Currency.Sum(x => x.WithdrawalAmount),
                CompanyAdjustAmount = Currency.Sum(x => x.CompanyAdjustAmount),
                BonusAmount = Currency.Sum(x => x.BonusAmount),
                BetAmount = Currency.Sum(x => x.BetAmount),
                Turnover = Currency.Sum(x => x.Turnover),
                WinLossAmount = Currency.Sum(x => x.WinLossAmount),
                NetProfit = Currency.Sum(x => x.NetProfit)
            })
            .ToArray();

        var datas = groupDatas.AsQueryable().CreatePagedResponse(request);

        return new()
        {
            Datas = datas,
            Totals = total
        };
    }
}