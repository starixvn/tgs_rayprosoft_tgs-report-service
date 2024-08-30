using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.Client.Models.PlayerDeposit;

public record GetPlayerFirstDepositDetailsRequest : PageableRequest
{
    public Guid BrandId { get; init; }
    public string AgentUsername { get; init; } = default!;
    public DateTimeOffset Date { get; init; }
}