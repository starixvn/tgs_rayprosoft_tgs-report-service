using System;

namespace Tgs.Report.Client.Models.GameProvider
{
    public class GameProviderRoundsModel
    {
        public Guid GameProviderId { get; set; }

        public string GameProviderName { get; set; }

        public string CurrencyCode { get; set; }

        public int TransactionCount { get; set; }

        public decimal BetAmount { get; set; }

        public decimal ValidBetAmount { get; set; }

        public decimal OriginalTurnover { get; set; }

        public decimal CompanyWinLoss { get; set; }
    }
}