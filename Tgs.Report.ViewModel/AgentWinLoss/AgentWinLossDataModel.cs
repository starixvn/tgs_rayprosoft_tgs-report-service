namespace Tgs.Report.ViewModel.AgentWinLoss;

public class AgentWinLossDataModel
{
    public string Id { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string MemberType { get; set; } = default!;
    public decimal Turnover { get; set; }
    public decimal WinLoss { get; set; }
    public decimal AgentWinLoss { get; set; }
    public decimal CompanyWinLoss { get; set; }
    public decimal Special { get; set; }
    public decimal AgentSpecial { get; set; }
    public decimal CompanySpecial { get; set; }
    public int TotalCount { get; set; }
}