using MediatR;
using Tgs.Report.Core.Models;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player;

internal class
    GetPlayerAllAgentRebateDetailsQueryHandle(IPlayerFactory playerFactory)
    : IStreamRequestHandler<GetPlayerAllAgentRebateDetailsRequest,
        PlayerAllAgentRebateDetailModel>
{
    public IAsyncEnumerable<PlayerAllAgentRebateDetailModel> Handle(GetPlayerAllAgentRebateDetailsRequest request,
        CancellationToken cancellationToken) =>
        playerFactory
            .GetPlayerAllAgentRebateDetailsQueryable(new GetPlayerAllAgentRebatesDetailQuery(request.BrandId,
                request.PlayerCode,
                request.SettleDateTime))
            .ToAsyncEnumerable();
}