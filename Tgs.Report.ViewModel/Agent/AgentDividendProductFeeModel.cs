using System;

namespace Tgs.Report.ViewModel.Agent;

public record AgentDividendProductFeeModel
{
    public decimal LiveDealerAmount { get; init; }
    public decimal SlotAmount { get; init; }
    public decimal LotteryAmount { get; init; }
    public decimal CardGameAmount { get; init; }
    public decimal SportsAmount { get; init; }
    public decimal VirtualSportsAmount { get; init; }
}