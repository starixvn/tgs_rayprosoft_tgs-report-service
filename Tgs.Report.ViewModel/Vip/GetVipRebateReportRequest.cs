using System;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.ViewModel.Vip;

public record GetVipRebateReportRequest : PageableRequest, IRequest<PagedResponse<VipRebateModel>>
{
    public Guid BrandId { get; init; }
    public string Username { get; set; } = default!;
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}