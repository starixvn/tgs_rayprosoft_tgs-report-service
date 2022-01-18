using System;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Round
{
    public class RoundAggregateModel
    {
        public Guid BrandId { get; set; }

        public Guid? AgentId { get; set; }

        public Guid GameProviderId { get; set; }

        public string PlayerCode { get; set; }

        public int GameId { get; set; }

        public GameType GameType { get; set; }

        public string Username { get; set; }

        public int Hour { get; set; }

        public int Minute { get; set; }

        public decimal BetAmount { get; set; }

        public decimal ValidBetAmount { get; set; }

        public decimal OriginalTurnover { get; set; }

        public decimal Turnover { get; set; }

        public decimal WinLoss { get; set; }

        public DateTimeOffset ReportDate { get; set; }
    }
}