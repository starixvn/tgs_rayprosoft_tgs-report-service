using System;

namespace Tgs.Report.ViewModel.Agent;

public record GetAgentContributionsDataRequest
{
    public Guid BrandId { get; set; }
    public Guid AgentId { get; set; }
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
}