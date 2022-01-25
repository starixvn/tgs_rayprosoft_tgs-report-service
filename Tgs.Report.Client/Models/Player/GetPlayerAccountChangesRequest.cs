using System;

namespace Tgs.Report.Client.Models.Player
{
    public record GetPlayerAccountChangesRequest : PageableRequest
    {
        public Guid BrandId { get; init; }
        public string PlayerCode { get; init; }
        public short? AccountChangeType { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}