using System;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.Vip
{
    public class GetVipRebateReportRequest : PageableRequest
    {
        public Guid BrandId { get; set; }
        public string Username { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}