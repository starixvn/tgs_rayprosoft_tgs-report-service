using MediatR;
using Tgs.Report.Core.Models;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player;

internal class
    GetPlayerAllAgentGameTypesQueryHandle : IStreamRequestHandler<GetPlayerAllAgentGameTypesRequest,
        PlayerAllAgentGameTypeModel>
{
    private readonly IPlayerFactory _playerFactory;

    public GetPlayerAllAgentGameTypesQueryHandle(IPlayerFactory playerFactory) => _playerFactory = playerFactory;

    public IAsyncEnumerable<PlayerAllAgentGameTypeModel> Handle(GetPlayerAllAgentGameTypesRequest request,
        CancellationToken cancellationToken) =>
        _playerFactory
            .GetPlayerAllAgentGameTypesQueryable(new GetPlayerAllAgentGameTypesQuery(request.BrandId,
                request.PlayerCode,
                request.SettleDateTime))
            .ToAsyncEnumerable();
}