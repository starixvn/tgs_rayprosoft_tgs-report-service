namespace Tgs.Report.Client.Models.Agent;

public record GetBreadcrumbRequest
{
    public Guid? AgentId { get; init; }
    public string? PlayerCode { get; init; }
    public Guid? RoundId { get; init; }
}