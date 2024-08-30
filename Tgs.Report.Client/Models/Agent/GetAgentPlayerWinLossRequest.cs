using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.Client.Models.Agent;

public record GetAgentPlayerWinLossRequest : PageableRequest
{
    public Guid AgentId { get; init; }
    public decimal AgentDividendPercentage { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}