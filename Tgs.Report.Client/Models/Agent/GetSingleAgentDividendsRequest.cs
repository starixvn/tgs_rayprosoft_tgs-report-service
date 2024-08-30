using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.Client.Models.Agent;

public record GetSingleAgentDividendsRequest : PageableRequest
{
    public Guid BrandId { get; init; }
    public Guid? AgentId { get; init; }
    public string? Username { get; init; }
    public short? Type { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}