using System;
using MediatR;

namespace Tgs.Report.ViewModel.Vip
{
    public record GetVipRebateReportRequest : PageableRequest, IRequest<PagedResponse<VipRebateModel>>
    {
        public Guid BrandId { get; init; }
        public string Username { get; set; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}