using System;

namespace Tgs.Report.Client.Models.Agent;

public record AgentDividendProductFeeResponse
{
    public decimal LiveDealerAmount { get; init; }
    public decimal SlotAmount { get; init; }
    public decimal LotteryAmount { get; init; }
    public decimal CardGameAmount { get; init; }
    public decimal SportsAmount { get; init; }
    public decimal VirtualSportsAmount { get; init; }
}