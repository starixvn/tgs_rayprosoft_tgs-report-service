using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.Client.Models.Round;

public record GetAgentPlayerRoundsRequest : PageableRequest
{
    public Guid AgentId { get; init; }
    public Guid BrandId { get; init; }
    public string? Username { get; init; }
    public string? PlayerCode { get; init; }
    public Guid? RoundId { get; init; }
    public string? ExternalRoundId { get; init; }
    public Guid? GameProviderId { get; init; }
    public short? Status { get; init; }
    public short? GameType { get; init; }
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }

    public DateTimeOffset? ClosedFrom { get; init; }
    public DateTimeOffset? ClosedTo { get; init; }
}