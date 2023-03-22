namespace Tgs.Report.Client.Models.Agent;

public record AgentPlayerWinLossModel
{
    public string Username { get; init; } = default!;
    public decimal LiveDealerWinLossAmount { get; init; }
    public decimal SlotWinLossAmount { get; init; }
    public decimal LotteryWinLossAmount { get; init; }
    public decimal CardGameWinLossAmount { get; init; }
    public decimal SportsWinLossAmount { get; init; }
    public decimal VirtualSportsWinLossAmount { get; init; }
}