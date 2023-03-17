using System;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Player;

public record PlayerGameTypeAggregateModel
{
    public Guid BrandId { get; init; }

    public Guid? AgentId { get; init; }

    public string PlayerCode { get; init; } = default!;

    public int? VipLevel { get; init; }

    public Guid GameProviderId { get; init; }

    public int GameId { get; init; }

    public GameType GameType { get; init; }

    public int Hour { get; init; }

    public decimal BetAmount { get; init; }

    public decimal ValidBetAmount { get; init; }

    public decimal OriginalTurnover { get; init; }

    public decimal Turnover { get; init; }

    public decimal WinLoss { get; init; }

    public decimal VipLiveDealerRebatePercentage { get; init; }

    public decimal VipSlotRebatePercentage { get; init; }

    public decimal VipCardGameRebatePercentage { get; init; }

    public decimal VipSportsRebatePercentage { get; init; }

    public decimal VipVirtualSportsRebatePercentage { get; init; }

    public decimal VipLotteryRebatePercentage { get; init; }

    public decimal VipRebateAmount { get; init; }

    public DateTimeOffset CreatedOn { get; init; }

    public DateTimeOffset ReportDate { get; init; }
}