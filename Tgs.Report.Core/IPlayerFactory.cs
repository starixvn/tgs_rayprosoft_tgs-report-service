using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core
{
    public interface IPlayerFactory
    {
        ValueTask<(PlayerAccountChangeModel[], int)>
            GetPlayerAccountChangeAsync(GetPlayerAccountChangesRequest request);
    }
}