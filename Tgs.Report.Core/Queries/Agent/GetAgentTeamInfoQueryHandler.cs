using MediatR;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent;

public class GetAgentTeamInfoQueryHandler(TgsContext context)
    : IRequestHandler<GetAgentTeamInfoRequest, PagedResponse<TeamInfoModel>>
{
    private readonly TgsContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public Task<PagedResponse<TeamInfoModel>> Handle(
        GetAgentTeamInfoRequest request,
        CancellationToken cancellationToken) =>
        _context.Players.Where(x => x.AgentId == request.AgentId)
            .Where(x => x.RegisteredOn != null)
            .GroupJoin(
                _context.PlayerAggregates.Where(x => x.AgentId == request.AgentId),
                p => p.PlayerCode,
                a => a.PlayerCode,
                (player, playerAggregate) => new
                {
                    player.Username,
                    player.RegisteredOn,
                    playerAggregate
                })
            .SelectMany(x => x.playerAggregate.DefaultIfEmpty(),
                (x, playerAggregate) => new
                {
                    x.Username,
                    x.RegisteredOn,
                    WinLossAmount = playerAggregate == null ? 0M : playerAggregate.WinLossAmount
                })
            .GroupBy(x => new
                {
                    x.Username,
                    x.RegisteredOn
                },
                (player, playerAggregate) =>
                    new TeamInfoModel
                    {
                        Username = player.Username,
                        RegisteredOn = player.RegisteredOn,
                        Contribution = playerAggregate.Sum(x => x.WinLossAmount)
                    }).CreatePagedResponseAsync(request);
}