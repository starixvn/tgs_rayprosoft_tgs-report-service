using System;
using Tgs.Report.Enum;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.Player
{
    public record GetPlayerAccountChangesRequest : PageableRequest
    {
        public Guid BrandId { get; init; }
        public string PlayerCode { get; init; }
        public AccountChangeType? AccountChangeType { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}