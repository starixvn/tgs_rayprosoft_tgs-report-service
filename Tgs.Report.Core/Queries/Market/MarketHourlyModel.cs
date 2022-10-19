namespace Tgs.Report.Core.Queries.Market;

public class MarketHourlyModel
{
    public DateTimeOffset ReportDate { get; set; }
    public int Hour { get; set; }
    public Guid? AgentId { get; set; }
    public decimal? ChiefAgentDividendPercentage { get; set; }
    public int RegisteredCount { get; set; }
    public int FirstFundInCount { get; set; }
    public decimal CompanyAdjustAmount { get; set; }
    public decimal NoIncludeNetProfitCompanyAdjustAmount { get; set; }
    public decimal BonusAdjustAmount { get; set; }
    public decimal NoIncludeNetProfitBonusAdjustAmount { get; set; }
    public decimal BetAmount { get; set; }
    public decimal OriginalTurnover { get; set; }
    public decimal Turnover { get; set; }
    public decimal WinLossAmount { get; set; }
    public decimal AllAgentRebateAmount { get; set; }
    public decimal RebateAmount { get; set; }
    public decimal PromotionAmount { get; set; }
    public decimal UpgradeAmount { get; set; }
    public decimal SeasonAmount { get; set; }
    public decimal BirthdayAmount { get; set; }
}