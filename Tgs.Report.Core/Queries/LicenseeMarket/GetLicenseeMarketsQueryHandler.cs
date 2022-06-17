using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.LicenseeMarket;

namespace Tgs.Report.Core.Queries.LicenseeMarket
{
    public class GetLicenseeMarketsQueryHandler : IRequestHandler<GetLicenseeMarketsRequest, LicenseeMarketsResponse>
    {
        private readonly TgsContext _context;

        public GetLicenseeMarketsQueryHandler(TgsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

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
                requestFrom.Offset).ToUniversalTime();

            var to = new DateTimeOffset(
                requestTo.Year,
                requestTo.Month,
                requestTo.Day,
                0,
                0,
                0,
                requestTo.Offset).ToUniversalTime();

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
                    CurrencyCode = string.Join(',', x.BrandCurrencies.Select(x => x.CurrencyCode).ToArray())
                })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            var withdrawals = await _context.WithdrawForms
                .Where(x => brands.Select(x => x.BrandId).Contains(x.BrandId))
                .Where(x => x.UserType == 1 && x.Status == 9000)
                .Where(x => x.CreatedOn >= from.ToUniversalTime() && x.CreatedOn < to.ToUniversalTime())
                .GroupBy(x => x.BrandId)
                .Select(withdrawal => new
                {
                    BrandId = withdrawal.Key,
                    WithdrawalAmount = -withdrawal.Sum(w => w.ActualAmount ?? w.Amount)
                })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            var playerAggregates = await _context.PlayerAggregates
                .Where(x => brands.Select(x => x.BrandId).Contains(x.BrandId))
                .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
                .GroupBy(x => x.BrandId)
                .Select(playerAggregate => new
                {
                    BrandId = playerAggregate.Key,
                    RegisteredCount = playerAggregate.Sum(m => m.RegisteredCount),
                    FirstFundInCount = playerAggregate.Sum(m => m.FirstFundInCount),
                    DepositAmount = playerAggregate.Sum(m => m.DepositAmount),
                    WithdrawalAmount = playerAggregate.Sum(m => m.WithdrawalAmount),
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
                    playerAggregates,
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
                        new LicenseeMarketDataModel
                        {
                            BarndId = market.BrandId,
                            BrandCode = market.BrandCode,
                            CurrencyCode = market.CurrencyCode,
                            RegisteredCount = playerAggregates.Sum(m => m.market?.RegisteredCount ?? 0),
                            FirstFundInCount = playerAggregates.Sum(m => m.market?.FirstFundInCount ?? 0),
                            DepositAmount = playerAggregates.Sum(m => m.market?.DepositAmount ?? 0),
                            WithdrawalAmount =
                                withdrawals.FirstOrDefault(x => x.BrandId == market.BrandId)?.WithdrawalAmount ?? 0,
                            CompanyAdjustAmount = playerAggregates.Sum(m => m.market?.CompanyAdjustAmount ?? 0),
                            BonusAmount = playerAggregates.Sum(m => m.market?.BonusAmount ?? 0),
                            BetAmount = playerAggregates.Sum(m => m.market?.BetAmount ?? 0),
                            Turnover = playerAggregates.Sum(m => m.market?.Turnover ?? 0),
                            WinLossAmount = playerAggregates.Sum(m => m.market?.WinLossAmount ?? 0),
                            NetProfit = playerAggregates.Sum(m => m.market?.NetProfit ?? 0)
                        });

            var total = groupDatas
                .GroupBy(c => c.CurrencyCode)
                .Select(Currency => new LicenseeMarketTotalModel
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

            return new LicenseeMarketsResponse
            {
                Datas = datas,
                Totals = total
            };
        }
    }
}