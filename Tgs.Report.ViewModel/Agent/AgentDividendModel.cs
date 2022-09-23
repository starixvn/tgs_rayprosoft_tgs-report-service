using System;

namespace Tgs.Report.ViewModel.Agent
{
    public record AgentDividendModel
    {
        public Guid Id { get; init; }

        public string Username { get; init; } = default!;

        public decimal AgentAdjustAmount { get; init; }

        public decimal BonusAdjustAmount { get; init; }

        public decimal BonusAmount { get; init; }

        public decimal BetAmount { get; init; }

        public decimal ValidBetAmount { get; init; }

        public decimal WinLossAmount { get; init; }

        public decimal NetProfit { get; init; }

        public decimal AgentDividendAmount { get; init; }

        public decimal FromPlayerAgentDividendAmount { get; init; }

        public decimal FromTeamAgentDividendAmount { get; init; }

        public decimal AgentDividendPercentage { get; init; }

        public decimal AgentCoveredBonus { get; init; }

        public decimal AgentCoveredBonusAdjust { get; init; }
    }
}