using System.Text.Json.Serialization;
using Tgs.Report.ViewModel.Round.BetHistories;

namespace Tgs.Report.ViewModel.Round
{
    public record GetRoundHistoryResponse
    {
        [JsonPropertyName("data")]
        public BaseData[] Data { get; init; }
    }
}