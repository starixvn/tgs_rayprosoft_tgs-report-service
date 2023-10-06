using MediatR;
using Tgs.Report.Core.Models;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player;

internal class
    GetPlayerAllAgentRebatesQueryHandle : IStreamRequestHandler<GetPlayerAllAgentRebatesRequest,
        PlayerAllAgentRebateModel>
{
    private readonly IPlayerFactory _playerFactory;

    public GetPlayerAllAgentRebatesQueryHandle(IPlayerFactory playerFactory) => _playerFactory = playerFactory;

    public IAsyncEnumerable<PlayerAllAgentRebateModel> Handle(GetPlayerAllAgentRebatesRequest request,
        CancellationToken cancellationToken) =>
        _playerFactory
            .GetPlayerAllAgentRebatesQueryable(new GetPlayerAllAgentRebatesQuery(request.BrandId,
                request.From,
                request.To,
                request.Username))
            .CreateResponseAsyncEnumerable(request);
}