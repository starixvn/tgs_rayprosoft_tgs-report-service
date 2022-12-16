namespace Tgs.Report.Core.Queries.Market;

internal class MarketHourlyModel
{
    public DateTimeOffset ReportDate { get; init; }
    public int Hour { get; init; }
    public Guid? AgentId { get; init; }
    public decimal? ChiefAgentDividendPercentage { get; init; }
    public int RegisteredCount { get; init; }
    public decimal CompanyAdjustAmount { get; init; }
    public decimal NoIncludeNetProfitCompanyAdjustAmount { get; init; }
    public decimal BonusAdjustAmount { get; init; }
    public decimal NoIncludeNetProfitBonusAdjustAmount { get; init; }
    public decimal BetAmount { get; init; }
    public decimal OriginalTurnover { get; init; }
    public decimal Turnover { get; init; }
    public decimal WinLossAmount { get; init; }
    public decimal AllAgentRebateAmount { get; init; }
    public decimal RebateAmount { get; init; }
    public decimal PromotionAmount { get; init; }
    public decimal UpgradeAmount { get; init; }
    public decimal SeasonAmount { get; init; }
    public decimal BirthdayAmount { get; init; }
    public int FirstFundInCount { get; init; }
}