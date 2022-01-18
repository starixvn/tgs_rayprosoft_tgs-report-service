using Tgs.Report.Repository.Configurations;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core
{
    public interface IPlayerGameAggregateFactory
    {
        ValueTask CreatePlayerGameAggregatesAsync(PlayerGameTypeAggregateModel[] request);
        ValueTask DeletePlayerGameAggregatesAsync(PlayerGameRebateAggregate[] markets);
    }
}