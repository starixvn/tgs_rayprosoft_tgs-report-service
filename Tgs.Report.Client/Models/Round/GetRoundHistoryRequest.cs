using System;

namespace Tgs.Report.Client.Models.Round
{
    public record GetRoundHistoryRequest
    {
        public string? GameProviderCode { get; init; }
        public string? PlayerCode { get; init; }
        public Guid RoundId { get; init; }
        public string? Language { get; init; }
    }
}