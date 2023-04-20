using System;

namespace Tgs.Report.ViewModel.Agent;

public record GetAgentWinLossDataRequest : PageableRequest
{
    public Guid BrandId { get; init; }
    public Guid AgentId { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}