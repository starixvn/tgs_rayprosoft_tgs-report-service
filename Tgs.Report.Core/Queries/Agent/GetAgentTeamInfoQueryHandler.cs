using MediatR;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent
{
    public class GetAgentTeamInfoQueryHandler : IRequestHandler<GetAgentTeamInfoRequest, PagedResponse<TeamInfoModel>>
    {
        private readonly TgsContext _context;

        public GetAgentTeamInfoQueryHandler(TgsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<PagedResponse<TeamInfoModel>> Handle(
            GetAgentTeamInfoRequest request,
            CancellationToken cancellationToken) =>
            _context.Players.Where(x => x.AgentId == request.AgentId)
                .Where(x => x.RegisteredOn != null).GroupJoin(
                    _context.PlayerAggregates.Where(x => x.AgentId == request.AgentId),
                    p => p.PlayerCode,
                    a => a.PlayerCode,
                    (player, playerAggregate) => new
                    {
                        player,
                        playerAggregate
                    })
                .SelectMany(x => x.playerAggregate.DefaultIfEmpty(),
                    (x, playerAggregate) => new
                    {
                        x.player.Username,
                        x.player.RegisteredOn,
                        playerAggregate
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
                            Contribution = playerAggregate.Sum(x
                                => x.playerAggregate == null ? 0M : x.playerAggregate.WinLossAmount)
                        }).CreatePagedResponseAsync(request);
    }
}