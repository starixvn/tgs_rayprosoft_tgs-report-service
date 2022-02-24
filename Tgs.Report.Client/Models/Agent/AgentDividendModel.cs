using System;

namespace Tgs.Report.Client.Models.Agent
{
    public record AgentDividendModel
    {
        public Guid Id { get; init; }

        public string Username { get; init; }

        public decimal DepositAmount { get; init; }

        public decimal WithdrawalAmount { get; init; }

        public decimal AdjustmentAmount { get; init; }

        public decimal BonusAmount { get; init; }

        public decimal BetAmount { get; init; }

        public decimal ValidBetAmount { get; init; }

        public decimal Turnover { get; init; }

        public decimal WinLossAmount { get; init; }

        public decimal NetProfit { get; init; }

        public decimal AgentDividendAmount { get; init; }

        public decimal FromPlayerAgentDividendAmount { get; init; }

        public decimal FromTeamAgentDividendAmount { get; init; }
    }
}