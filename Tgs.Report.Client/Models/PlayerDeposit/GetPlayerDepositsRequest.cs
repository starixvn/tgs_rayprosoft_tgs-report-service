using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.Client.Models.PlayerDeposit;

public record GetPlayerDepositsRequest : PageableRequest
{
    public Guid BrandId { get; init; }
    public string AgentUsername { get; init; } = default!;
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}