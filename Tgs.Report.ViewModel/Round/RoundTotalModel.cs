namespace Tgs.Report.ViewModel.Round;

public record RoundTotalModel
{
    public decimal? BetAmount { get; init; }

    public decimal? ValidBetAmount { get; init; }

    public decimal? WinLoss { get; init; }
}