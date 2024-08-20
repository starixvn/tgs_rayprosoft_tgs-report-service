using System.Text.Json;
using MediatR;
using Tgs.Report.Core.Models;
using Tgs.Report.Enum;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player;

public class
    GetPlayersAccountChangesQueryHandler(IPlayerFactory playerFactory) : IRequestHandler<GetPlayersAccountChangesRequest,
    PagedResponse<PlayersAccountChangeModel>>
{
    public async Task<PagedResponse<PlayersAccountChangeModel>> Handle(
        GetPlayersAccountChangesRequest request,
        CancellationToken cancellationToken)
    {
        var playersAccountChanges = await playerFactory
            .GetPlayersAccountChangesQueryable(new GetPlayersAccountChangesQuery(request.BrandId, request.From,
                request.To, request.Username, request.TransactionType)).CreatePagedResponseAsync(request);

        if (playersAccountChanges.Items == null)
        {
            return playersAccountChanges;
        }

        foreach (var item in playersAccountChanges.Items)
        {
            if (item.TransactionType is not (TransactionType.CompanyAdjust or TransactionType.AgentAdjust
                or TransactionType.SystemAdjust or TransactionType.TransferAdjust or TransactionType.BonusAdjust))
            {
                continue;
            }

            if (item.Description is null)
            {
                item.Description = "";
            }
            else
            {
                try
                {
                    item.Description = JsonSerializer.Deserialize<AdjustRemarkModel>(item.Description)?.Remark;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        return playersAccountChanges;
    }
}