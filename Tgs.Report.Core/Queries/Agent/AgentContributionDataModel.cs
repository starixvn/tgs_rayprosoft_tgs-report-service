namespace Tgs.Report.Core.Queries.Agent;

internal class AgentContributionDataModel
{
    public Guid AgentId { get; init; }
    public string PlayerCode { get; init; } = default!;
    public decimal CompanyAdjustAmount { get; init; }
    public decimal AgentAdjustAmount { get; init; }
    public decimal BonusAdjustAmount { get; init; }
    public decimal BonusAmount { get; init; }
    public decimal BetAmount { get; init; }
    public decimal OriginalTurnover { get; init; }
    public decimal Turnover { get; init; }
    public decimal WinLossAmount { get; init; }
    public decimal NetProfit { get; init; }
    public decimal UpgradeAmount { get; init; }
    public decimal SeasonAmount { get; init; }
    public decimal BirthdayAmount { get; init; }
}