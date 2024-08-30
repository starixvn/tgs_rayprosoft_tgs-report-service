using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.Client.Models.Agent;

public record GetAgentAccountChangesRequest : PageableRequest
{
    public Guid BrandId { get; init; }
    public Guid AgentId { get; init; }
    public short? TransactionType { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}