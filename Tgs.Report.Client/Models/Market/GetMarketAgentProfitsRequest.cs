using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.Client.Models.Market;

public record GetMarketAgentProfitsRequest : PageableRequest
{
    public Guid BrandId { get; init; }
    public DateTimeOffset Date { get; init; }
}