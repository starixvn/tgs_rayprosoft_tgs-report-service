namespace Tgs.Report.ViewModel.Agent;

public class AgentWinLossDataModel
{
    public string Username { get; set; }
    public string MemberType { get; set; }
    public decimal Turnover { get; set; }
    public decimal WinLoss { get; set; }
    public decimal AgentWinLoss { get; set; }
    public decimal CompanyWinLoss { get; set; }
    public int TotalCount { get; set; }
}