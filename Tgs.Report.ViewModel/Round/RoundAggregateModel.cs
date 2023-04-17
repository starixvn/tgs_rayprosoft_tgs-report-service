using System;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Round;

public record RoundAggregateModel
{
    public Guid BrandId { get; init; }

    public Guid? AgentId { get; init; }

    public Guid GameProviderId { get; init; }

    public string? PlayerCode { get; init; }

    public int GameId { get; init; }

    public GameType GameType { get; init; }

    public string? Username { get; init; }

    public int Hour { get; init; }

    public int Minute { get; init; }

    public decimal BetAmount { get; init; }

    public decimal ValidBetAmount { get; init; }

    public decimal OriginalTurnover { get; init; }

    public decimal Turnover { get; init; }

    public decimal WinLoss { get; init; }

    public DateTimeOffset ReportDate { get; init; }
}