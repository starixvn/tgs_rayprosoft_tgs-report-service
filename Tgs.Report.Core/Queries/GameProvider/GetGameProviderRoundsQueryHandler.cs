using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel.GameProvider;

namespace Tgs.Report.Core.Queries.GameProvider
{
    internal class
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
            var rounds = await _repository.Rounds
                .Where(x => x.CreatedOn >= request.From && x.CreatedOn < request.To)
                .Where(x => x.BrandId == request.BrandId)
                .Join(_repository.Games,
                    round => round.GameId,
                    game => game.Id,
                    (round, game) => new
                    {
                        round,
                        game.Type
                    })
                .Where(request.GameType is not null, x => x.Type == request.GameType)
                .Join(_repository.GameProviders,
                    group => group.round.GameProviderId,
                    gp => gp.Id,
                    (group, gp) => new
                    {
                        group.round,
                        GameProviderName = gp.Name,
                        GameProviderCode = gp.Code
                    })
                .Select(x => new
                {
                    x.round,
                    x.GameProviderName,
                    x.GameProviderCode
                })
                .Where(request.GameProviderCode, x => x.GameProviderCode == request.GameProviderCode)
                .Join(_repository.Brands,
                    group => group.round.BrandId,
                    brand => brand.Id,
                    (group, brand) => new
                    {
                        group.round,
                        group.GameProviderName,
                        group.GameProviderCode,
                        brand.BrandCurrencies.FirstOrDefault().CurrencyCode
                    })
                .Select(x => new
                {
                    x.round.GameProviderId,
                    x.round.BetAmount,
                    x.round.ValidBetAmount,
                    x.round.Turnover,
                    x.round.WinLoss,
                    x.GameProviderName,
                    x.GameProviderCode,
                    x.CurrencyCode
                }).ToArrayAsync(cancellationToken);

            var roundGroups = rounds.GroupBy(x => new
                {
                    x.GameProviderId,
                    x.GameProviderName,
                    x.CurrencyCode
                },
                (group, items) => new GameProviderRoundsModel
                {
                    GameProviderId = group.GameProviderId,
                    GameProviderName = group.GameProviderName,
                    CurrencyCode = group.CurrencyCode,
                    TransactionCount = items.Count(),
                    BetAmount = items.Sum(i => i.BetAmount ?? 0M),
                    ValidBetAmount = items.Sum(i => i.ValidBetAmount ?? 0M),
                    OriginalTurnover = items.Sum(i => i.Turnover ?? 0M),
                    CompanyWinLoss = items.Sum(i => i.WinLoss ?? 0M)
                }).ToArray();

            return new GetGameProviderRoundsResponse
            {
                Datas = roundGroups.AsQueryable().CreatePagedResponse(request),
                Totals = roundGroups.GroupBy(x => new { x.CurrencyCode },
                    (group, items) => new GameProviderRoundTotalModel
                    {
                        CurrencyCode = group.CurrencyCode,
                        TransactionCount = items.Sum(i => i.TransactionCount),
                        BetAmount = items.Sum(i => i.BetAmount),
                        ValidBetAmount = items.Sum(i => i.ValidBetAmount),
                        OriginalTurnover = items.Sum(i => i.OriginalTurnover),
                        CompanyWinLoss = items.Sum(i => i.CompanyWinLoss)
                    }).ToArray()
            };
        }
    }
}