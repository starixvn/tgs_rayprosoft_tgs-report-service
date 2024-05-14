namespace Tgs.Report.Client.Models.AgentWinLoss;

public record GetAgentTotalDetailRequest
{
    public Guid BrandId { get; init; }
    public Guid AgentId { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}