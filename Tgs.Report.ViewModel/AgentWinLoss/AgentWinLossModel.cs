namespace Tgs.Report.ViewModel.AgentWinLoss;

public record AgentWinLossModel
{
    public string Id { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string MemberType { get; set; } = default!;
    public int RoundCount { get; set; }
    public decimal Turnover { get; set; }
    public decimal WinLoss { get; set; }
    public decimal AgentWinLoss { get; set; }
    public decimal CompanyWinLoss { get; set; }
    public decimal Special { get; set; }
    public decimal AgentSpecial { get; set; }
    public decimal CompanySpecial { get; set; }
    public decimal PlatformChargeAmount { get; set; }
    public decimal FeeAmount { get; set; }
    public decimal AgentTotal => GetAgentTotal();
    public decimal CompanyTotal => GetCompanyTotal();

    private decimal GetAgentTotal()
    {
        return AgentSpecial + AgentWinLoss;
    }

    private decimal GetCompanyTotal()
    {
        return CompanySpecial + CompanyWinLoss;
    }
}