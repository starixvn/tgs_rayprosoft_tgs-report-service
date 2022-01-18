using System;
using MediatR;

namespace Tgs.Report.ViewModel.Vip
{
    public class GetVipReportRequest : PageableRequest, IRequest<PagedResponse<VipModel>>
    {
        public Guid BrandId { get; set; }
        public string Username { get; set; }
        public int? Level { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}