using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel.GameProvider;

namespace Tgs.Report.Core.Queries.GameProvider
{
    public class
        GetGameProviderRoundsQueryHandler : IRequestHandler<GetGameProviderRoundsRequest,
            GetGameProviderRoundsResponse>
    {
        private readonly ITgsRepository _repository;

        public GetGameProviderRoundsQueryHandler(ITgsRepository repository)
        {
            _repository = repository;
        }

        public async Task<GetGameProviderRoundsResponse> Handle(
            GetGameProviderRoundsRequest request,
            CancellationToken cancellationToken)
        {
            var transactionCounts = await _repository.Rounds.Where(x => x.BrandId == request.BrandId)
                .Where(x => x.ClosedOn >= request.From && x.ClosedOn < request.To)
                .Join(_repository.Games,
                    t => t.GameId,
                    g => g.Id,
                    (round, game) => new
                    {
                        round.GameProviderId,
                        round.GameId,
                        round.CurrencyCode,
                        game.Type,
                        game.GameProvider.Code
                    })
                .Where(request.GameProviderCode, x => x.Code == request.GameProviderCode)
                .Where(request.GameType is not null, x => x.Type == (short)request.GameType)
                .GroupBy(x => new
                    {
                        x.GameProviderId,
                        x.Type,
                        x.GameId,
                        x.CurrencyCode
                    },
                    (group, items) => new
                    {
                        group.GameProviderId,
                        group.Type,
                        group.GameId,
                        group.CurrencyCode,
                        Count = items.Count()
                    }).ToArrayAsync(cancellationToken).ConfigureAwait(false);

            var response = await _repository.PlayerGameRebateAggregates
                .Where(x => x.ReportDate >= request.From && x.ReportDate < request.To)
                .Where(x => x.BrandId == request.BrandId)
                .Where(request.GameType is not null, x => x.GameType == (short)request.GameType)
                .Join(_repository.GameProviders,
                    aggregate => aggregate.GameProviderId,
                    gp => gp.Id,
                    (aggregate, gp) => new
                    {
                        aggregate,
                        gp
                    })
                .Select(x => new
                {
                    x.aggregate,
                    x.gp
                })
                .Where(request.GameProviderCode, x => x.gp.Code == request.GameProviderCode)
                .Join(_repository.Brands,
                    group => group.aggregate.BrandId,
                    brand => brand.Id,
                    (group, brand) => new
                    {
                        group.aggregate,
                        group.gp,
                        brand
                    })
                .Select(x => new
                {
                    x.aggregate,
                    x.gp,
                    x.brand.BrandCurrencies.FirstOrDefault().CurrencyCode
                })
                .GroupBy(x => new
                    {
                        x.aggregate.GameProviderId,
                        x.gp.Name,
                        x.CurrencyCode
                    },
                    (group, items) => new GameProviderRoundsModel
                    {
                        GameProviderId = group.GameProviderId,
                        GameProviderName = group.Name,
                        CurrencyCode = group.CurrencyCode,
                        BetAmount = items.Sum(i => i.aggregate.BetAmount),
                        ValidBetAmount = items.Sum(i => i.aggregate.ValidBetAmount),
                        OriginalTurnover = items.Sum(i => i.aggregate.OriginalTurnover),
                        CompanyWinLoss = items.Sum(i => i.aggregate.WinLoss)
                    }).ToArrayAsync(cancellationToken);

            foreach (var item in response)
            {
                item.TransactionCount =
                    transactionCounts
                        .Where(t => t.GameProviderId == item.GameProviderId && t.CurrencyCode == item.CurrencyCode)
                        .Sum(t => t.Count);
            }

            return new GetGameProviderRoundsResponse
            {
                Datas = response.AsQueryable().CreatePagedResponse(request),
                Totals = response.GroupBy(x => new { x.CurrencyCode },
                    (group, items) => new GameProviderRoundTotalModel
                    {
                        CurrencyCode = group.CurrencyCode,
                        TransactionCount =
                            transactionCounts
                                .Where(x => x.CurrencyCode == group.CurrencyCode).Sum(x => x.Count),
                        BetAmount = items.Sum(i => i.BetAmount),
                        ValidBetAmount = items.Sum(i => i.ValidBetAmount),
                        OriginalTurnover = items.Sum(i => i.OriginalTurnover),
                        CompanyWinLoss = items.Sum(i => i.CompanyWinLoss)
                    }).ToArray()
            };
        }
    }
}