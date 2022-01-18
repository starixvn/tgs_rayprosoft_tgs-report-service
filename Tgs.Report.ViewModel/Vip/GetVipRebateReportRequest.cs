using System;
using MediatR;

namespace Tgs.Report.ViewModel.Vip
{
    public class GetVipRebateReportRequest : PageableRequest, IRequest<PagedResponse<VipRebateModel>>
    {
        public Guid BrandId { get; set; }
        public string Username { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}