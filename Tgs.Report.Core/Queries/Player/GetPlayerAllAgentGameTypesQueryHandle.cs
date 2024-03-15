using MediatR;
using Tgs.Report.Core.Models;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player;

internal class
    GetPlayerAllAgentGameTypesQueryHandle(IPlayerFactory playerFactory)
    : IStreamRequestHandler<GetPlayerAllAgentRebateDetailGameTypesRequest,
        PlayerAllAgentRebateDetailGameTypeModel>
{
    public IAsyncEnumerable<PlayerAllAgentRebateDetailGameTypeModel> Handle(GetPlayerAllAgentRebateDetailGameTypesRequest request,
        CancellationToken cancellationToken) =>
        playerFactory
            .GetPlayerAllAgentRebateDetailGameTypesQueryable(new GetPlayerAllAgentRebateDetailGameTypesQuery(request.BrandId,
                request.PlayerCode,
                request.SettleDateTime))
            .ToAsyncEnumerable();
}