using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.Client.Models.Vip;

public record GetVipRebateReportRequest : PageableRequest
{
    public Guid BrandId { get; init; }
    public string Username { get; init; } = default!;
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}