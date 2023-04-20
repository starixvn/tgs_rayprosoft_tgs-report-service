namespace Tgs.Report.Client.Models.Agent;

public record GetAgentWinLossRequest : PageableRequest
{
    public Guid BrandId { get; init; }
    public Guid AgentId { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}