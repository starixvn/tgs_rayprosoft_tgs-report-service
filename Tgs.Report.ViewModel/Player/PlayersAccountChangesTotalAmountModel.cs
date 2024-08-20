using System;

namespace Tgs.Report.ViewModel.Player
{
    public class PlayersAccountChangesTotalAmountModel
    {
        public int TotalCount { get; set; }
        public decimal Amount { get; set; }
        public decimal BeforeBalance { get; set; }
        public decimal AfterBalance { get; set; }
    }
}
