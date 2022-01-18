namespace Tgs.Report.ViewModel.Agent
{
    public class ExportAgentDividendModel
    {
        public string Username { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal WithdrawalAmount { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal BetAmount { get; set; }
        public decimal ValidBetAmount { get; set; }
        public decimal WinLossAmount { get; set; }
        public decimal NetProfit { get; set; }
        public decimal AgentDividendAmount { get; set; }
    }
}