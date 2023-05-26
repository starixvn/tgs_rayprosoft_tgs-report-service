namespace Tgs.Report.Client.Models.AgentWinLoss;

public record BreadcrumbModel
{
    public string Name { get; set; } = default!;
    public string Id { get; set; } = default!;
    public string Type { get; set; } = default!;
}