using Tgs.Report.ViewModel.Round.BetHistories;

namespace Tgs.Report.Client.Models.Round
{
    public record GetRoundHistoryResponse
    {
        public string GameProviderName { get; init; }
        public string GameName { get; init; }
        public string ExternalRoundId { get; init; }
        public decimal BetAmount { get; init; }
        public decimal Turnover { get; init; }
        public decimal Winloss { get; init; }
        public string GameResult { get; init; }
        public BaseData[] Data { get; init; }
    }
}