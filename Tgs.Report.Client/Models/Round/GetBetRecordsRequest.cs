using System;

namespace Tgs.Report.Client.Models.Round
{
    public record GetBetRecordsRequest : PageableRequest
    {
        public Guid BrandId { get; init; }
        public string PlayerCode { get; init; } = default!;
        public Guid? GameProviderId { get; init; }
        public short? Status { get; init; }
        public short? GameType { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}