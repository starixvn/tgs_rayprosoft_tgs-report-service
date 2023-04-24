using System;

namespace Tgs.Report.ViewModel.AgentWinLoss;

public record AgentPlayerSpecialDetailModel
{
    public string Special { get; set; } = default!;
    public decimal Amount { get; set; }
}