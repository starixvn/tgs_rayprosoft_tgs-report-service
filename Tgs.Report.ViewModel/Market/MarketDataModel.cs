using System;

namespace Tgs.Report.ViewModel.Market
{
    public class MarketDataModel
    {
        public Guid BrandId { get; set; }
        public string PlayerCode { get; set; }
        public int Hour { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset ReportDate { get; set; }
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