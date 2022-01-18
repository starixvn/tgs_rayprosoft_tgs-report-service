namespace Tgs.Report.ViewModel.Vip
{
    public class ExportVipModel
    {
        public string Username { get; set; }
        public int? Level { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal WithdrawalAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public decimal RebateAmount { get; set; }
        public decimal ValidBetAmount { get; set; }
        public decimal WinLossAmount { get; set; }
        public decimal NetProfit { get; set; }
    }
}