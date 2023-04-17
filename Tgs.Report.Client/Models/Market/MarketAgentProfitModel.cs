namespace Tgs.Report.Client.Models.Market;

public record MarketAgentProfitModel
{
    public string MasterAgentName { get; init; } = default!;

    public decimal Percentage { get; init; }

    public decimal DirectPlayerNetProfit { get; init; }

    public decimal TeamNetProfit { get; init; }

    public decimal AgentProfit { get; init; }

    public decimal CompanyAdjustment { get; init; }

    public decimal CompanyProfit { get; init; }
}