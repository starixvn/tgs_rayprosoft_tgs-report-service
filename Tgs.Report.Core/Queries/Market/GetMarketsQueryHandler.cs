using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Market;

namespace Tgs.Report.Core.Queries.Market
{
    internal class GetMarketsQueryHandler : IRequestHandler<GetMarketsRequest, PagedResponse<MarketModel>>
    {
        private readonly ITgsRepository _repository;

        public GetMarketsQueryHandler(ITgsRepository repository)
        {
            _repository = repository;
        }

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
                requestFrom.Offset).ToUniversalTime();

            var to = new DateTimeOffset(requestTo.Year,
                requestTo.Month,
                requestTo.Day,
                0,
                0,
                0,
                requestTo.Offset).ToUniversalTime();

            var dates = Enumerable.Range(0, to.Subtract(from).Days)
                .Select(x => from.AddDays(x))
                .ToArray();

            var withdrawals = await _repository.WithdrawForms
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.UserType == 1 && x.Status == 9000)
                .Where(x => x.CreatedOn >= from && x.CreatedOn < to)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            var withdrawalByDates = withdrawals.GroupBy(x => x.CreatedOn.ToOffset(offset).Date,
                (date, withdrawals) => new
                {
                    Date = date,
                    WithdrawalAmount = withdrawals.Sum(w => w.ActualAmount ?? w.Amount)
                });

            return await dates.GroupJoin(_repository.PlayerAggregates
                        .Where(x => x.BrandId == request.BrandId)
                        .Where(x => x.ReportDate >= from && x.ReportDate < to),
                    date => date,
                    market => market.ReportDate,
                    (date, markets) => new
                    {
                        date,
                        markets
                    })
                .SelectMany(x => x.markets.DefaultIfEmpty(),
                    (x, market) => new
                    {
                        ReportDate = x.date,
                        market
                    })
                .GroupBy(x => new { x.ReportDate },
                    (market, markets) =>
                        new MarketModel
                        {
                            ReportDate = market.ReportDate,
                            RegisteredCount = markets.Sum(m => m.market?.RegisteredCount ?? 0),
                            FirstFundInCount = markets.Sum(m => m.market?.FirstFundInCount ?? 0),
                            DepositAmount = markets.Sum(m => m.market?.DepositAmount ?? 0M),
                            WithdrawalAmount =
                                -withdrawalByDates.FirstOrDefault(w => w.Date == market.ReportDate.Date)
                                    ?.WithdrawalAmount ??
                                0M,
                            AdjustmentAmount = markets.Sum(m => m.market?.AdjustmentAmount ?? 0M),
                            BonusAmount =
                                markets.Sum(
                                    m => m.market == null ? 0 : m.market.RebateAmount + m.market.PromotionAmount),
                            BetAmount = markets.Sum(m => m.market?.BetAmount ?? 0M),
                            ValidBetAmount = markets.Sum(m => m.market?.OriginalTurnover ?? 0M),
                            Turnover = markets.Sum(m => m.market?.Turnover ?? 0M),
                            WinLossAmount = markets.Sum(m => m.market?.WinLossAmount ?? 0M),
                            NetProfit = markets.Sum(m
                                => m.market == null
                                    ? 0M
                                    : m.market.WinLossAmount - m.market.AdjustmentAmount - m.market.RebateAmount -
                                    m.market.PromotionAmount),
                            AdditionalExpenses = markets.Sum(m => m.market?.AllAgentRebateAmount ?? 0M)
                        })
                .CreatePagedResponseAsync(dates.Length);
        }
    }
}