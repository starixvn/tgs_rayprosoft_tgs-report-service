using System;

namespace Tgs.Report.ViewModel.LicenseeMarket
{
    public class LicenseeMarketDataModel
    {
        public Guid BarndId { get; set; }
        public string BrandCode { get; set; }
        public string CurrencyCode { get; set; }
        public int RegisteredCount { get; set; }
        public int FirstFundInCount { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal WithdrawalAmount { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal BetAmount { get; set; }
        public decimal Turnover { get; set; }
        public decimal WinLossAmount { get; set; }
        public decimal NetProfit { get; set; }
    }
}