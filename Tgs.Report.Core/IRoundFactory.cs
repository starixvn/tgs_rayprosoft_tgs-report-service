using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core
{
    public interface IRoundFactory
    {
        Task<RoundAggregateModel[]> GenerateRoundAggregateAsync(DateTimeOffset start, DateTimeOffset end);

        Task<RoundAggregateModel[]> GetMinuteRoundAggregateAsync(DateTimeOffset start, DateTimeOffset end);
    }
}