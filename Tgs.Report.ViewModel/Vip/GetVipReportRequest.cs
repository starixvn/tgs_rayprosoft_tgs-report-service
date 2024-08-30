using System;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.ViewModel.Vip;

public record GetVipReportRequest : PageableRequest, IRequest<PagedResponse<VipModel>>
{
    public Guid BrandId { get; init; }
    public string? Username { get; init; }
    public int? Level { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}