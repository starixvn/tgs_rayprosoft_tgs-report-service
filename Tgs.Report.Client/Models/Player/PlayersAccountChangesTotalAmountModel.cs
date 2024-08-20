namespace Tgs.Report.Client.Models.Player
{
    public class PlayersAccountChangesTotalAmountModel
    {
        public int TotalCount { get; set; }
        public decimal Amount { get; init; }
        public decimal BeforeBalance { get; init; }
        public decimal AfterBalance { get; init; }
    }
}
