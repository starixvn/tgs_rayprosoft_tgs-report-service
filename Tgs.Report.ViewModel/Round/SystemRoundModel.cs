using System;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Round;

public record SystemRoundModel
{
    public string? PlayerName { get; init; }
    public string? PlayerCode { get; init; }
    public string? ExternalRoundId { get; init; }
    public DateTimeOffset? ExternalCreatedOn { get; init; }
    public DateTimeOffset? ExternalClosedOn { get; init; }
    public Guid Id { get; init; }
    public DateTimeOffset CreatedOn { get; init; }
    public DateTimeOffset? ClosedOn { get; init; }
    public string? GameProviderName { get; init; }
    public string? SubGameProviderName { get; init; }
    public string? GameName { get; init; }
    public ClientRoundStatus Status { get; init; }
    public string? CurrencyCode { get; init; }
    public decimal? BetAmount { get; init; }
    public decimal? ValidBetAmount { get; init; }
    public decimal? Turnover { get; init; }
    public decimal? SettleAmount { get; init; }
    public decimal? WinLoss { get; init; }
    public string? AdditionalData { get; init; }
    public string? GameProviderCode { get; init; }
    public int GameId { get; init; }
    public decimal? OriginalTurnover { get; init; }
    public decimal EffectiveBettingPercentage { get; init; }
}