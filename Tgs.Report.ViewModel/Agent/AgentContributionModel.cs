using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Agent
{
    public record AgentContributionModel
    {
        public string Username { get; init; } = default!;
        public AccountType Type { get; init; }
        public decimal DepositAmount { get; init; }
        public decimal WithdrawalAmount { get; init; }
        public decimal AdjustmentAmount { get; init; }
        public decimal BonusAmount { get; init; }
        public decimal BetAmount { get; init; }
        public decimal ValidBetAmount { get; init; }
        public decimal Turnover { get; init; }
        public decimal WinLossAmount { get; init; }
        public decimal NetProfit { get; init; }
    }
}