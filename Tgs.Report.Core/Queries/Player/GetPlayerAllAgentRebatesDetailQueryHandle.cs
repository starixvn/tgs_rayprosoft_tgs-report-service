using MediatR;
using Tgs.Report.Core.Models;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player;

internal class
    GetPlayerAllAgentRebatesDetailQueryHandle : IStreamRequestHandler<GetPlayerAllAgentRebatesDetailsRequest,
        PlayerAllAgentRebateDetailModel>
{
    private readonly IPlayerFactory _playerFactory;

    public GetPlayerAllAgentRebatesDetailQueryHandle(IPlayerFactory playerFactory) => _playerFactory = playerFactory;

    public IAsyncEnumerable<PlayerAllAgentRebateDetailModel> Handle(GetPlayerAllAgentRebatesDetailsRequest request,
        CancellationToken cancellationToken) =>
        _playerFactory
            .GetPlayerAllAgentRebatesDetailQueryable(new GetPlayerAllAgentRebatesDetailQuery(request.BrandId,
                request.PlayerCode,
                request.SettleDateTime))
            .ToAsyncEnumerable();
}