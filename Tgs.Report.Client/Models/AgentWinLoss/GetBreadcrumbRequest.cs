namespace Tgs.Report.Client.Models.AgentWinLoss;

public record GetBreadcrumbRequest
{
    public Guid? AgentId { get; init; }
    public string? PlayerCode { get; init; }
    public Guid? RoundId { get; init; }
}