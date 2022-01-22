using System;

namespace Tgs.Report.ViewModel
{
    public record FillInPlayerBucketsRequest
    {
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
        public PlayerTurnoverModel[] PlayerTurnovers { get; init; }
    }

    public record PlayerTurnoverModel
    {
        public string PlayerCode { get; init; } = default!;
        public decimal Turnover { get; init; }
    }
}