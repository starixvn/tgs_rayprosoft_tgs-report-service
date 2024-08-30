using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.Client.Models.Market;

public record GetMarketsRequest : PageableRequest
{
    public Guid BrandId { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}