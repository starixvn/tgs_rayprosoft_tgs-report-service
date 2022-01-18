using System;

namespace Tgs.Report.ViewModel.Vip
{
    public class VipRebateModel
    {
        public DateTimeOffset ReportDate { get; set; }
        public decimal LiveDealerAmount { get; set; }
        public decimal SlotAmount { get; set; }
        public decimal LotteryAmount { get; set; }
        public decimal CardGameAmount { get; set; }
        public decimal SportsAmount { get; set; }
        public decimal VirtualSportsAmount { get; set; }
        public decimal LiveDealerTurnover { get; set; }
        public decimal SlotTurnover { get; set; }
        public decimal LotteryTurnover { get; set; }
        public decimal CardGameTurnover { get; set; }
        public decimal SportsTurnover { get; set; }
        public decimal VirtualSportsTurnover { get; set; }
        public decimal LiveDealerRebatePercentage { get; set; }
        public decimal SlotRebatePercentage { get; set; }
        public decimal LotteryRebatePercentage { get; set; }
        public decimal CardGameRebatePercentage { get; set; }
        public decimal SportsRebatePercentage { get; set; }
        public decimal VirtualSportsRebatePercentage { get; set; }
    }
}