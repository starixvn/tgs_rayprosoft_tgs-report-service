using MediatR;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent
{
    public class GetAgentTeamInfoQueryHandler : IRequestHandler<GetAgentTeamInfoRequest, PagedResponse<TeamInfoModel>>
    {
        private readonly ITgsRepository _repository;

        public GetAgentTeamInfoQueryHandler(ITgsRepository repository)
        {
            _repository = repository;
        }

        public Task<PagedResponse<TeamInfoModel>> Handle(
            GetAgentTeamInfoRequest request,
            CancellationToken cancellationToken) =>
            _repository.Players.Where(x => x.AgentId == request.AgentId)
                .Where(x => x.RegisteredOn != null).GroupJoin(
                    _repository.PlayerAggregates.Where(x => x.AgentId == request.AgentId),
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