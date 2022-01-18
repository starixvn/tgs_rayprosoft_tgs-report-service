namespace Tgs.Report.ViewModel.Agent
{
    public class AgentPlayerWinLossModel
    {
        public string Username { get; set; }
        public decimal LiveDealerWinLossAmount { get; set; }
        public decimal SlotWinLossAmount { get; set; }
        public decimal LotteryWinLossAmount { get; set; }
        public decimal CardGameWinLossAmount { get; set; }
        public decimal SportsWinLossAmount { get; set; }
        public decimal VirtualSportsWinLossAmount { get; set; }
    }
}