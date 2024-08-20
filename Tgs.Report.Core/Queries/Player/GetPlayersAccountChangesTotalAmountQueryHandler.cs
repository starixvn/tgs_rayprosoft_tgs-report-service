using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Core.Models;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player;

public class
    GetPlayersAccountChangesTotalAmountQueryHandler(IPlayerFactory playerFactory) : IRequestHandler<GetPlayersAccountChangesTotalAmountRequest,
    PlayersAccountChangesTotalAmountModel>
{
    public async Task<PlayersAccountChangesTotalAmountModel> Handle(
        GetPlayersAccountChangesTotalAmountRequest request,
        CancellationToken cancellationToken)
    {
        return await playerFactory
            .GetPlayersAccountChangesQueryable(new GetPlayersAccountChangesQuery(request.BrandId, request.From,
                request.To, request.Username, request.TransactionType))
            .GroupBy(x => 1)
            .Select(x => new PlayersAccountChangesTotalAmountModel
            {
                TotalCount = x.Count(),
                Amount = x.Sum(s => s.Amount),
                BeforeBalance = x.Sum(s => s.BeforeBalance),
                AfterBalance = x.Sum(s => s.AfterBalance)
            }).FirstAsync(cancellationToken);
    }
}