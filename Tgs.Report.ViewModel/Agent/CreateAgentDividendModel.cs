using System;

namespace Tgs.Report.ViewModel.Agent
{
    public record CreateAgentDividendModel
    {
        public int? Id { get; init; }

        public int? ReferencedAgentDividendId { get; init; }

        public string AgentUsername { get; init; } = default!;

        public Guid AgentId { get; init; }

        public Guid BrandId { get; init; }

        public DateTimeOffset From { get; init; }

        public DateTimeOffset To { get; init; }

        public decimal PromotionAmount { get; init; }

        public decimal RebateAmount { get; init; }

        public decimal DepositAmount { get; init; }

        public decimal WithdrawalAmount { get; init; }

        public decimal AdjustmentAmount { get; init; }

        public decimal BonusAmount { get; init; }

        public decimal BetAmount { get; init; }

        public decimal ValidBetAmount { get; init; }

        public decimal OriginalTurnover { get; init; }

        public decimal Turnover { get; init; }

        public decimal WinLossAmount { get; init; }

        public decimal NetProfit { get; init; }

        public decimal LiveDealerWinLoss { get; init; }

        public decimal SlotWinLoss { get; init; }

        public decimal FishingWinLoss { get; init; }

        public decimal ArcadeWinLoss { get; init; }

        public decimal TableGameWinLoss { get; init; }

        public decimal LotteryWinLoss { get; init; }

        public decimal CardGameWinLoss { get; init; }

        public decimal SportsWinLoss { get; init; }

        public decimal ESportsWinLoss { get; init; }

        public decimal VirtualSportsWinLoss { get; init; }

        public decimal AgentDividendPercentage { get; init; }

        public decimal AgentDividendAmount { get; init; }

        public decimal? BeforeDividendAmount { get; init; }

        public DateTimeOffset CreatedOn { get; init; }

        public bool AutoSend { get; init; }

        public AgentDividendMultipleLayerModel[] AgentDividendMultipleLayerModels { get; init; }
    }
}