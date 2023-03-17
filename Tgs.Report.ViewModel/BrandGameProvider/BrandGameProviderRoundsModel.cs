using System;

namespace Tgs.Report.ViewModel.BrandGameProvider;

public record BrandGameProviderRoundsModel
{
    public Guid GameProviderId { get; init; }

    public string GameProviderName { get; init; } = default!;

    public string CurrencyCode { get; init; } = default!;

    public int TransactionCount { get; set; }

    public decimal BetAmount { get; init; }

    public decimal ValidBetAmount { get; init; }

    public decimal OriginalTurnover { get; init; }

    public decimal CompanyWinLoss { get; init; }
}