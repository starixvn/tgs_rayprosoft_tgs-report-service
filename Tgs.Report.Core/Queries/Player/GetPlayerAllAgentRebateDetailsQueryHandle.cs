using MediatR;
using Tgs.Report.Core.Models;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player;

internal class
    GetPlayerAllAgentRebateDetailsQueryHandle : IStreamRequestHandler<GetPlayerAllAgentRebateDetailsRequest,
        PlayerAllAgentRebateDetailModel>
{
    private readonly IPlayerFactory _playerFactory;

    public GetPlayerAllAgentRebateDetailsQueryHandle(IPlayerFactory playerFactory) => _playerFactory = playerFactory;

    public IAsyncEnumerable<PlayerAllAgentRebateDetailModel> Handle(GetPlayerAllAgentRebateDetailsRequest request,
        CancellationToken cancellationToken) =>
        _playerFactory
            .GetPlayerAllAgentRebateDetailsQueryable(new GetPlayerAllAgentRebatesDetailQuery(request.BrandId,
                request.PlayerCode,
                request.SettleDateTime))
            .ToAsyncEnumerable();
}