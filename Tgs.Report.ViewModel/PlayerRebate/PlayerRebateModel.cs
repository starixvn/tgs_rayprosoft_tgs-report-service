using System;

namespace Tgs.Report.ViewModel.PlayerRebate
{
    public record PlayerRebateModel
    {
        public int Id { get; init; }
        public Guid BrandId { get; init; }
        public Guid? AgentId { get; init; }
        public int GameId { get; init; }
        public string PlayerCode { get; init; } = default!;
        public DateTimeOffset CreatedOn { get; init; }
        public DateTimeOffset ReportDate { get; init; }
        public decimal SportsTurnover { get; init; }
        public decimal SportsRebateAmount { get; init; }
        public decimal VirtualSportsTurnover { get; init; }
        public decimal VirtualSportsRebateAmount { get; init; }
        public decimal LiveDealerTurnover { get; init; }
        public decimal LiveDealerRebateAmount { get; init; }
        public decimal CardGameTurnover { get; init; }
        public decimal CardGameRebateAmount { get; init; }
        public decimal SlotTurnover { get; init; }
        public decimal SlotRebateAmount { get; init; }
        public decimal LotteryTurnover { get; init; }
        public decimal LotteryRebateAmount { get; init; }
        public decimal VipLiveDealerRebatePercentage { get; init; }
        public decimal VipSlotRebatePercentage { get; init; }
        public decimal VipCardGameRebatePercentage { get; init; }
        public decimal VipSportsRebatePercentage { get; init; }
        public decimal VipVirtualSportsRebatePercentage { get; init; }
        public decimal VipLotteryRebatePercentage { get; init; }
        public decimal RebateAmount { get; init; }
    }
}