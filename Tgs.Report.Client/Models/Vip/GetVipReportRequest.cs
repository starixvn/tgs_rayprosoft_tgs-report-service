using System;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.Vip
{
    public record GetVipReportRequest : PageableRequest
    {
        public Guid BrandId { get; init; }
        public string Username { get; init; }
        public int? Level { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}