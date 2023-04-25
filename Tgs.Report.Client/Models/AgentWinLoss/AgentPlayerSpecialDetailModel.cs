namespace Tgs.Report.Client.Models.AgentWinLoss;

public record AgentPlayerSpecialDetailModel
{
    public decimal Bonus { get; set; }
    public decimal BonusAdjust { get; set; }
    public decimal Special { get; set; }
}