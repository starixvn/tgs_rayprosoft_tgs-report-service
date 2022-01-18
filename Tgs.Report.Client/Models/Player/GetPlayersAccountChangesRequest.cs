using System;
using Tgs.Report.Enum;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.Player
{
    public class GetPlayersAccountChangesRequest : PageableRequest
    {
        public Guid BrandId { get; set; }
        public string Username { get; set; }
        public TransactionType? TransactionType { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}