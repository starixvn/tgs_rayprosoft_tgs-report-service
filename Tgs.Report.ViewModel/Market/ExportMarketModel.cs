namespace Tgs.Report.ViewModel.Market
{
    public record ExportMarketModel
    {
        public string ReportDate { get; init; } = default!;
        public int RegisteredCount { get; init; }
        public int FirstFundInCount { get; init; }
        public decimal DepositAmount { get; init; }
        public decimal WithdrawalAmount { get; init; }
        public decimal AdjustmentAmount { get; init; }
        public decimal BonusAmount { get; init; }
        public decimal BetAmount { get; init; }
        public decimal ValidBetAmount { get; init; }
        public decimal WinLossAmount { get; init; }
        public decimal NetProfit { get; init; }
    }
}