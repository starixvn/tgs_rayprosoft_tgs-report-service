namespace Tgs.Report.ViewModel.AgentWinLoss;

public record AgentPlayerSpecialFeesModel
{
    public decimal Bonus { get; set; }
    public decimal BonusAdjust { get; set; }
    public decimal CompanyAdjust { get; set; }
    public decimal AgentAdjust { get; set; }
}