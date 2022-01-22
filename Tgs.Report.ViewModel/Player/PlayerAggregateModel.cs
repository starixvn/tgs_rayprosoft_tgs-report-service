using System;

namespace Tgs.Report.ViewModel.Player
{
    public record PlayerAggregateModel
    {
        public Guid BrandId { get; init; }

        public Guid? AgentId { get; init; }

        public string PlayerCode { get; init; } = default!;

        public int Hour { get; init; }

        public DateTimeOffset CreatedOn { get; init; }

        public DateTimeOffset ReportDate { get; init; }

        public int RegisteredCount { get; init; }

        public int FirstFundInCount { get; init; }

        public decimal DepositAmount { get; init; }

        public decimal WithdrawalAmount { get; init; }

        public decimal AdjustmentAmount { get; init; }

        public decimal BonusAmount { get; init; }

        public decimal RebateAmount { get; init; }

        public decimal DividendAmount { get; init; }

        public decimal PromotionAmount { get; init; }

        public decimal BetAmount { get; init; }

        public decimal ValidBetAmount { get; init; }

        public decimal OriginalTurnover { get; init; }

        public decimal Turnover { get; init; }

        public decimal WinLossAmount { get; init; }

        public decimal AllAgentRebateAmount { get; init; }
    }
}