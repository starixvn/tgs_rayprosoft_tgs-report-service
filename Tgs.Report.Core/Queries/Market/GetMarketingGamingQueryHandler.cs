using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Marketing;

namespace Tgs.Report.Core.Queries.Market;

public class GetMarketingGamingQueryHandler : IRequestHandler<GetMarketingGamingDetailRequest,
    PagedResponse<MarketingGamingDetailModel>>
{
    private readonly ITgsRepository _repository;

    public GetMarketingGamingQueryHandler(ITgsRepository repository) => _repository = repository;

    public async Task<PagedResponse<MarketingGamingDetailModel>> Handle(
        GetMarketingGamingDetailRequest request,
        CancellationToken cancellationToken)
    {
        var offset = new TimeSpan(8, 0, 0);
        var requestDate = request.Date.ToOffset(offset);

        var reportDate = new DateTimeOffset(requestDate.Year,
            requestDate.Month,
            requestDate.Day,
            0,
            0,
            0,
            requestDate.Offset);

        var agent = await _repository.Agents
            .FirstOrDefaultAsync(x => x.BrandId == request.BrandId && x.Username == request.AgentUsername,
                cancellationToken).ConfigureAwait(false);

        if (agent == null)
        {
            return new()
            {
                Valid = true,
                Items = Array.Empty<MarketingGamingDetailModel>()
            };
        }

        return await _repository.PlayerGameRebateAggregates
            .Where(x => x.BrandId == request.BrandId && x.ReportDate == reportDate.ToUniversalTime() &&
                x.AgentId == agent.Id)
            .Join(_repository.Players, aggregate => aggregate.PlayerCode, player => player.PlayerCode,
                (aggregate, player) => new
                {
                    player.Username,
                    aggregate.PlayerCode,
                    aggregate.GameProviderId,
                    aggregate.GameId,
                    aggregate.WinLoss
                })
            .Where(request.PlayerUsername, x => x.Username == request.PlayerUsername)
            .Join(_repository.Games, aggregate => aggregate.GameId, game => game.Id,
                (aggregate, game) => new
                {
                    game.SubGameProviderCode,
                    aggregate.Username,
                    aggregate.PlayerCode,
                    aggregate.GameProviderId,
                    aggregate.GameId,
                    aggregate.WinLoss
                })
            .Join(_repository.SubGameProviders, aggregate => aggregate.SubGameProviderCode, sub => sub.Code,
                (aggregate, sub) => new
                {
                    aggregate.Username,
                    aggregate.PlayerCode,
                    aggregate.GameProviderId,
                    aggregate.GameId,
                    aggregate.WinLoss,
                    SubGameProviderName = sub.Name
                })
            .Join(_repository.GameProviders, aggregate => aggregate.GameProviderId, provider => provider.Id,
                (aggregate, provider) => new
                {
                    aggregate.Username,
                    aggregate.PlayerCode,
                    aggregate.GameProviderId,
                    aggregate.GameId,
                    aggregate.WinLoss,
                    aggregate.SubGameProviderName,
                    GameProviderName = provider.Name
                })
            .GroupBy(x => new
            {
                x.Username,
                x.PlayerCode,
                x.GameProviderName,
                x.SubGameProviderName
            }, (player, group) => new MarketingGamingDetailModel
            {
                Username = player.Username,
                GameProviderName = player.GameProviderName,
                SubGameProviderName = player.SubGameProviderName,
                WinLossAmount = group.Sum(x => x.WinLoss)
            })
            .CreatePagedResponseAsync(request).ConfigureAwait(false);
    }
}