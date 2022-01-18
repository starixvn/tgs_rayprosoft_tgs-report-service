using System;
using Tgs.Report.Enum;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.Player
{
    public class GetPlayerAccountChangesRequest : PageableRequest
    {
        public Guid BrandId { get; set; }
        public string PlayerCode { get; set; }
        public AccountChangeType? AccountChangeType { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}