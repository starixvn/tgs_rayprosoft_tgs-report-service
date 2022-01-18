using System;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.Vip
{
    public class GetVipReportRequest : PageableRequest
    {
        public Guid BrandId { get; set; }
        public string Username { get; set; }
        public int? Level { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}