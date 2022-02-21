using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;

namespace Tgs.Report.ViewModel.Round
{
    public record GetRoundHistoryRequest : IRequest<JsonDocument?>
    {
        [JsonPropertyName("gpcode")]
        public string? GameProviderCode { get; set; }

        [JsonPropertyName("playercode")]
        public string? PlayerCode { get; set; }

        [JsonPropertyName("roundid")]
        public Guid RoundId { get; set; }

        /// <summary>
        ///     player's culture code
        /// </summary>
        [JsonPropertyName("language")]
        public string? Language { get; set; }
    }
}