namespace Tgs.Report.ViewModel.BrandGameProvider;

public record BrandGameProviderTotalModel
{
    public string CurrencyCode { get; init; } = default!;
    public int TransactionCount { get; init; }
    public decimal BetAmount { get; init; }
    public decimal ValidBetAmount { get; init; }
    public decimal Turnover { get; init; }
    public decimal CompanyWinLoss { get; init; }
    public int PlayerCount { get; set; }
}