namespace Tgs.Report.ViewModel.Agent;

public record BreadcrumbModel
{
    public string Name { get; set; } = default!;
    public string Id { get; set; } = default!;
    public string Type { get; set; } = default!;
}