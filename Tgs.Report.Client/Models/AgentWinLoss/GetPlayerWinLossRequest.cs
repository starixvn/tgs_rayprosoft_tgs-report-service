using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.Client.Models.AgentWinLoss;

public record GetPlayerWinLossRequest : PageableRequest
{
    public string PlayerCode { get; init; } = default!;
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}