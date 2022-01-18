namespace Tgs.Report.ViewModel.GameProvider
{
    public class GameProviderRoundTotalModel
    {
        public string CurrencyCode { get; set; }

        public int TransactionCount { get; set; }

        public decimal BetAmount { get; set; }

        public decimal ValidBetAmount { get; set; }

        public decimal OriginalTurnover { get; set; }

        public decimal CompanyWinLoss { get; set; }
    }
}