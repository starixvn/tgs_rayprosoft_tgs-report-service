namespace Tgs.Report.Client.Models.AgentWinLoss;

public record GetPlayerWinLossRequest : PageableRequest
{
    public string PlayerCode { get; init; } = default!;
    public decimal PlatformCharge { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}