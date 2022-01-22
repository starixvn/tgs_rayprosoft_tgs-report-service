namespace Tgs.Report.Client.Models.GameProvider
{
    public record GameProviderRoundTotalModel
    {
        public string CurrencyCode { get; init; }

        public int TransactionCount { get; init; }

        public decimal BetAmount { get; init; }

        public decimal ValidBetAmount { get; init; }

        public decimal OriginalTurnover { get; init; }

        public decimal CompanyWinLoss { get; init; }
    }
}