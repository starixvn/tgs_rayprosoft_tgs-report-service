using Tgs.Report.Repository.Configurations;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core
{
    public interface IPlayerAggregateFactory
    {
        ValueTask CreatePlayerAggregatesAsync(PlayerAggregateModel[] request);
        ValueTask DeletePlayerAggregatesAsync(PlayerAggregate[] markets);
    }
}