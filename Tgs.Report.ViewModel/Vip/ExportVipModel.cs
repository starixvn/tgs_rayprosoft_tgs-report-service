namespace Tgs.Report.ViewModel.Vip
{
    public record ExportVipModel
    {
        public string? Username { get; init; }
        public int? Level { get; init; }
        public decimal DepositAmount { get; init; }
        public decimal WithdrawalAmount { get; init; }
        public decimal BonusAmount { get; init; }
        public decimal AdjustmentAmount { get; init; }
        public decimal RebateAmount { get; init; }
        public decimal ValidBetAmount { get; init; }
        public decimal WinLossAmount { get; init; }
        public decimal NetProfit { get; init; }
    }
}