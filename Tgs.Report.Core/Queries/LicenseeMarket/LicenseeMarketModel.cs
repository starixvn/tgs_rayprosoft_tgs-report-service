namespace Tgs.Report.Core.Queries.LicenseeMarket;

public class LicenseeMarketModel
{
    public Guid BrandId { get; set; }
    public int RegisteredCount { get; set; }
    public int FirstFundInCount { get; set; }
    public decimal CompanyAdjustAmount { get; set; }
    public decimal BetAmount { get; set; }
    public decimal NetProfit { get; set; }
    public decimal Turnover { get; set; }
    public decimal WinLossAmount { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal RebateAmount { get; set; }
    public decimal PromotionAmount { get; set; }
    public decimal UpgradeAmount { get; set; }
    public decimal SeasonAmount { get; set; }
    public decimal BirthdayAmount { get; set; }
}