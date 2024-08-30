using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.Client.Models.Round;

public record GetOpenRoundsRequest : PageableRequest
{
    public string? ExternalRoundId { get; init; }

    public Guid? GameProviderId { get; init; }

    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }
}