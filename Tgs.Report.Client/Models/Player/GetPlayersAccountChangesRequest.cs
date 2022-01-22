using System;
using Tgs.Report.Enum;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.Player
{
    public record GetPlayersAccountChangesRequest : PageableRequest
    {
        public Guid BrandId { get; init; }
        public string Username { get; init; }
        public TransactionType? TransactionType { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}