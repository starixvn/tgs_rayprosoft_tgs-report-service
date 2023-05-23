using System;

namespace Tgs.Report.ViewModel.Round;

public record ExportRoundsModel
{
    public string PlayerName { get; init; } = default!;
    public string ExternalRoundId { get; init; } = default!;
    public Guid Id { get; init; }
    public string CreatedOn { get; init; } = default!;
    public string? ClosedOn { get; init; }
    public string GameProviderName { get; init; } = default!;
    public string SubGameProviderName { get; init; } = default!;
    public string GameName { get; init; } = default!;
    public string CurrencyCode { get; init; } = default!;
    public decimal BetAmount { get; init; }
    public decimal ValidBetAmount { get; init; }
    public decimal Turnover { get; init; }
    public decimal SettleAmount { get; init; }
    public decimal WinLoss { get; init; }
    public string? PlayerCode { get; init; }
    public string? ExternalCreatedOn { get; init; }
    public string? ExternalClosedOn { get; init; }
}