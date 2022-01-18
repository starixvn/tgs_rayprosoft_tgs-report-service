using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core
{
    public interface IRoundAggregateFactory
    {
        ValueTask<(PlayerAggregateModel[] playerAggregates, PlayerGameTypeAggregateModel[] playerAggregateByGameType)>
            GetPlayerAggregatesAsync(
                DateTimeOffset start,
                DateTimeOffset end);
    }
}