using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.Client.Models.Round;

public record GetClosedRoundsRequest : PageableRequest
{
    public string? ExternalRoundId { get; init; }

    public Guid? GameProviderId { get; init; }
}