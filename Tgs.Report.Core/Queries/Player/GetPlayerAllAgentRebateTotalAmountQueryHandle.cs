using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Core.Models;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player;

internal class
    GetPlayerAllAgentRebateTotalAmountQueryHandle : IRequestHandler<GetPlayerAllAgentRebateTotalAmountRequest, PlayerAllAgentRebateTotalAmountModel>
{
    private readonly IPlayerFactory _playerFactory;

    public GetPlayerAllAgentRebateTotalAmountQueryHandle(IPlayerFactory playerFactory) => _playerFactory = playerFactory;

    public async Task<PlayerAllAgentRebateTotalAmountModel> Handle(GetPlayerAllAgentRebateTotalAmountRequest request,
        CancellationToken cancellationToken)
    {
        var rebates = await _playerFactory.GetPlayerAllAgentRebatesQueryable(new GetPlayerAllAgentRebatesQuery(
                request.BrandId,
                request.From,
                request.To,
                request.Username))
            .ToArrayAsync(cancellationToken);

        return new PlayerAllAgentRebateTotalAmountModel
        {
            TotalCount = rebates.Length,
            ValidBet = rebates.Sum(x => x.ValidBet),
            Rebate = rebates.Sum(x => x.Rebate)
        };
    }
}