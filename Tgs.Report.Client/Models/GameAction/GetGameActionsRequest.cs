namespace Tgs.Report.Client.Models.GameAction;

public record GetGameActionsRequest : PageableRequest
{
    public Guid RoundId { get; init; }
}