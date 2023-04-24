namespace Tgs.Report.Client.Models.AgentWinLoss;

public record AgentPlayerSpecialDetailModel
{
    public string Special { get; set; } = default!;
    public decimal Amount { get; set; }
}