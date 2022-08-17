using System;

namespace Tgs.Report.ViewModel.Round
{
    public record ExportSystemRoundsModel
    {
        public string? PlayerName { get; init; }
        public string? PlayerCode { get; init; }
        public string? ExternalRoundId { get; init; }
        public DateTimeOffset? ExternalCreatedOn { get; init; }
        public DateTimeOffset? ExternalClosedOn { get; init; }
        public Guid Id { get; init; }
        public DateTimeOffset CreatedOn { get; init; } = default!;
        public DateTimeOffset? ClosedOn { get; init; }
        public decimal? BetAmount { get; init; }
        public decimal? WinLoss { get; init; }
    }
}