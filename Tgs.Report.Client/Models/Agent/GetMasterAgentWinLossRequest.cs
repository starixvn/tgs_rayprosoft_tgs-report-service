namespace Tgs.Report.Client.Models.Agent;

public record GetMasterAgentWinLossRequest : PageableRequest
{
    public Guid BrandId { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}