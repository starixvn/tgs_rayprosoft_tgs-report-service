namespace Tgs.Report.Client.Models.Round
{
    public record SystemRoundTotalModel
    {
        public decimal? BetAmount { get; init; }

        public decimal? ValidBetAmount { get; init; }
        public decimal? Turnover { get; init; }

        public decimal? SettleAmount { get; init; }

        public decimal? WinLoss { get; init; }
    }
}