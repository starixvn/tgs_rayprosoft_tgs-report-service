using Tgs.Report.Core.Models;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core;

public interface IPlayerFactory
{
    ValueTask<(PlayerAccountChangeModel[], int)> GetPlayerAccountChangeAsync(GetPlayerAccountChangesQuery request);

    ValueTask<decimal> GetPlayerAccountChangeTotalAmountAsync(GetPlayerAccountChangesTotalAmountQuery request);

    IQueryable<PlayerAllAgentRebateModel> GetPlayerAllAgentRebatesQueryable(GetPlayerAllAgentRebatesQuery request);

    IQueryable<PlayerAllAgentRebateDetailModel> GetPlayerAllAgentRebateDetailsQueryable(GetPlayerAllAgentRebatesDetailQuery request);

    IQueryable<PlayerAllAgentRebateDetailGameTypeModel> GetPlayerAllAgentRebateDetailGameTypesQueryable(GetPlayerAllAgentRebateDetailGameTypesQuery request);

    IQueryable<PlayersAccountChangeModel> GetPlayersAccountChangesQueryable(GetPlayersAccountChangesQuery request);

}