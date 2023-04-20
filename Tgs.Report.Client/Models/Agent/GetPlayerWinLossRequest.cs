namespace Tgs.Report.Client.Models.Agent;

public record GetPlayerWinLossRequest : PageableRequest
{
    public string PlayerCode { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}