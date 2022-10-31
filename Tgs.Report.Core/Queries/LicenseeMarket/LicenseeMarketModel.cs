namespace Tgs.Report.Core.Queries.LicenseeMarket;

public class LicenseeMarketModel
{
    public Guid BrandId { get; init; }
    public decimal CompanyAdjustAmount { get; init; }
    public decimal BetAmount { get; init; }
    public decimal NetProfit { get; init; }
    public decimal Turnover { get; init; }
    public decimal WinLossAmount { get; init; }
    public decimal BonusAmount { get; init; }
    public decimal RebateAmount { get; init; }
    public decimal PromotionAmount { get; init; }
    public decimal UpgradeAmount { get; init; }
    public decimal SeasonAmount { get; init; }
    public decimal BirthdayAmount { get; init; }
}