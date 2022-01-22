using System;

namespace Tgs.Report.ViewModel.GameProvider
{
    public record GameProviderRoundsModel
    {
        public Guid GameProviderId { get; init; }

        public string GameProviderName { get; init; } = default!;

        public string CurrencyCode { get; init; } = default!;

        public int TransactionCount { get; init; }

        public decimal BetAmount { get; init; }

        public decimal ValidBetAmount { get; init; }

        public decimal OriginalTurnover { get; init; }

        public decimal CompanyWinLoss { get; init; }
    }
}