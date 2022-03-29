using System;

namespace Tgs.Report.ViewModel.ResellerMarket
{
    public record ResellerMarketDataModel
    {
        public Guid BarndId { get; init; }
        public string BrandCode { get; init; } = default!;
        public string CurrencyCode { get; init; } = default!;
        public int RegisteredCount { get; init; }
        public int FirstFundInCount { get; init; }
        public decimal DepositAmount { get; init; }
        public decimal WithdrawalAmount { get; init; }
        public decimal CompanyAdjustAmount { get; init; }
        public decimal BonusAmount { get; init; }
        public decimal BetAmount { get; init; }
        public decimal Turnover { get; init; }
        public decimal WinLossAmount { get; init; }
        public decimal NetProfit { get; init; }
    }
}