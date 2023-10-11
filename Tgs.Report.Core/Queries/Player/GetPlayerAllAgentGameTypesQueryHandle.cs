using MediatR;
using Tgs.Report.Core.Models;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player;

internal class
    GetPlayerAllAgentGameTypesQueryHandle : IStreamRequestHandler<GetPlayerAllAgentRebateDetailGameTypesRequest,
        PlayerAllAgentRebateDetailGameTypeModel>
{
    private readonly IPlayerFactory _playerFactory;

    public GetPlayerAllAgentGameTypesQueryHandle(IPlayerFactory playerFactory) => _playerFactory = playerFactory;

    public IAsyncEnumerable<PlayerAllAgentRebateDetailGameTypeModel> Handle(GetPlayerAllAgentRebateDetailGameTypesRequest request,
        CancellationToken cancellationToken) =>
        _playerFactory
            .GetPlayerAllAgentRebateDetailGameTypesQueryable(new GetPlayerAllAgentRebateDetailGameTypesQuery(request.BrandId,
                request.PlayerCode,
                request.SettleDateTime))
            .ToAsyncEnumerable();
}