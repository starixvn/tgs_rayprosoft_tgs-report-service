using MediatR;
using Tgs.Report.Core.Models;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player;

public class GetPlayerAccountChangeTotalAmountQueryHandler(IPlayerFactory playerFactory) :
    IRequestHandler<GetPlayerAccountChangesTotalAmountRequest, decimal>
{
    private readonly IPlayerFactory _playerFactory = playerFactory ?? throw new ArgumentNullException(nameof(playerFactory));

    public async Task<decimal> Handle(
        GetPlayerAccountChangesTotalAmountRequest request,
        CancellationToken cancellationToken)
        => await _playerFactory
            .GetPlayerAccountChangeTotalAmountAsync(new GetPlayerAccountChangesTotalAmountQuery
            {
                BrandId = request.BrandId,
                PlayerCode = request.PlayerCode,
                AccountChangeType = request.AccountChangeType,
                From = request.From,
                To = request.To
            })
            .ConfigureAwait(false);
}