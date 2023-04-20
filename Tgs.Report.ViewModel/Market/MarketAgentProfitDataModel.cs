using System;

namespace Tgs.Report.ViewModel.Market;

public record MarketAgentProfitDataModel
{
    public string MasterAgentName { get; init; }

    public decimal Percentage { get; init; }

    public decimal DirectPlayerNetProfit { get; init; }

    public decimal TeamNetProfit { get; init; }

    public decimal AgentProfit { get; init; }

    public decimal CompanyAdjustment { get; init; }

    public decimal CompanyProfit { get; init; }

    public int TotalCount { get; init; }
}