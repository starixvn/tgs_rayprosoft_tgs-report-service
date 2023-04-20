namespace Tgs.Report.Client.Models.Agent;

public record AgentWinLossModel
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string MemberType { get; set; }
    public decimal Turnover { get; set; }
    public decimal WinLoss { get; set; }
    public decimal AgentWinLoss { get; set; }
    public decimal CompanyWinLoss { get; set; }
    public decimal Special { get; set; }
    public decimal AgentSpecial { get; set; }
    public decimal CompanySpecial { get; set; }
    public decimal AgentTotal { get; set; }
    public decimal CompanyTotal { get; set; }
}