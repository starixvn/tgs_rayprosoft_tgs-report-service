using System.Text.Json.Serialization;
using Tgs.Report.ViewModel.Round.BetHistories;

namespace Tgs.Report.ViewModel.Round
{
    public record GetRoundHistoryResponse
    {
        [JsonPropertyName("gpname")]
        public string GameProviderName { get; init; }

        [JsonPropertyName("gname")]
        public string GameName { get; init; }

        [JsonPropertyName("externalroundid")]
        public string ExternalRoundId { get; init; }

        [JsonPropertyName("betamount")]
        public decimal BetAmount { get; init; }

        [JsonPropertyName("turnover")]
        public decimal Turnover { get; init; }

        [JsonPropertyName("winloss")]
        public decimal Winloss { get; init; }

        [JsonPropertyName("result")]
        public string GameResult { get; init; }

        [JsonPropertyName("data")]
        public BaseData[] Data { get; init; }
    }
}