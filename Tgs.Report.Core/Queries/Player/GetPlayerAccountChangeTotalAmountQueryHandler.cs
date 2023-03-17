using MediatR;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player;

public class GetPlayerAccountChangeTotalAmountQueryHandler :
    IRequestHandler<GetPlayerAccountChangesTotalAmountRequest, decimal>
{
    private readonly IPlayerFactory _playerFactory;

    public GetPlayerAccountChangeTotalAmountQueryHandler(IPlayerFactory playerFactory)
        => _playerFactory = playerFactory ?? throw new ArgumentNullException(nameof(playerFactory));

    public async Task<decimal> Handle(
        GetPlayerAccountChangesTotalAmountRequest request,
        CancellationToken cancellationToken)
        => await _playerFactory
            .GetPlayerAccountChangeTotalAmountAsync(request)
            .ConfigureAwait(false);
}