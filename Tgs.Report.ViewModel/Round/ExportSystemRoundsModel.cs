using System;

namespace Tgs.Report.ViewModel.Round;

public record ExportSystemRoundsModel
{
    public string? PlayerName { get; init; }
    public string? PlayerCode { get; init; }
    public string? ExternalRoundId { get; init; }
    public string? ExternalCreatedOn { get; init; }
    public string? ExternalClosedOn { get; init; }
    public Guid Id { get; init; }
    public string CreatedOn { get; init; } = default!;
    public string? ClosedOn { get; init; }
    public decimal? BetAmount { get; init; }
    public decimal? WinLoss { get; init; }
}