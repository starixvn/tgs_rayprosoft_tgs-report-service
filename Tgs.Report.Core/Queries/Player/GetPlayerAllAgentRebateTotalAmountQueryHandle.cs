using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Core.Models;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player;

internal class
    GetPlayerAllAgentRebateTotalAmountQueryHandle(IPlayerFactory playerFactory)
    : IRequestHandler<GetPlayerAllAgentRebateTotalAmountRequest, PlayerAllAgentRebateTotalAmountModel>
{
    public async Task<PlayerAllAgentRebateTotalAmountModel> Handle(GetPlayerAllAgentRebateTotalAmountRequest request,
        CancellationToken cancellationToken)
    {
        var rebates = await playerFactory.GetPlayerAllAgentRebatesQueryable(new GetPlayerAllAgentRebatesQuery(
                request.BrandId,
                request.From,
                request.To,
                request.Username,
                request.PlayerCode))
            .ToArrayAsync(cancellationToken);

        return new PlayerAllAgentRebateTotalAmountModel
        {
            TotalCount = rebates.Length,
            ValidBet = rebates.Sum(x => x.ValidBet),
            Rebate = rebates.Sum(x => x.Rebate)
        };
    }
}