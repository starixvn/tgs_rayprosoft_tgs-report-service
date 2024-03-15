using MediatR;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.GameProvider;

namespace Tgs.Report.Core.Queries.GameProvider;

public class
    GetGameProviderRoundsQueryHandler(
        ITgsService<GameProviderAggregate> gameProviderAggregateService,
        ITgsService<Repository.Entities.GameProvider> gameProviderService)
    : IRequestHandler<GetGameProviderRoundsRequest,
        PagedResponse<GameProviderRoundsModel>>
{
    public Task<PagedResponse<GameProviderRoundsModel>> Handle(
        GetGameProviderRoundsRequest request,
        CancellationToken cancellationToken)
        => gameProviderAggregateService.GetQueries()
            .Where(request.GameProviderId, x => x.GameProviderId == request.GameProviderId)
            .Where(request.CurrencyCode, x => x.CurrencyCode == request.CurrencyCode)
            .Where(x => x.ReportDate >= request.From && x.ReportDate < request.To)
            .Join(gameProviderService.GetQueries(),
                a => a.GameProviderId,
                g => g.Id,
                (round, gameProvider) => new
                {
                    round.GameProviderId,
                    round.CurrencyCode,
                    round.TransactionCount,
                    round.BetAmount,
                    round.ValidBetAmount,
                    round.Turnover,
                    round.WinLoss,
                    GameProviderCode = gameProvider.Code,
                    GameProviderName = gameProvider.Name
                })
            .GroupBy(x => new
            {
                x.GameProviderId,
                x.GameProviderName,
                x.CurrencyCode
            }, (group, items) => new GameProviderRoundsModel
            {
                GameProviderId = group.GameProviderId,
                GameProviderName = group.GameProviderName,
                CurrencyCode = group.CurrencyCode,
                TransactionCount = items.Sum(i => i.TransactionCount),
                BetAmount = items.Sum(i => i.BetAmount),
                OriginalTurnover = items.Sum(i => i.Turnover),
                ValidBetAmount = items.Sum(i => i.ValidBetAmount),
                CompanyWinLoss = items.Sum(i => i.WinLoss)
            }).CreatePagedResponseAsync(request);
}