using System;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.AgentWinLoss;

public record ProductFeeDataModel
{
    public Guid AgentId { get; set; }
    public decimal ProduceFeePercentage { get; set; }
    public GameType GameType { get; set; }
    public decimal WinLossAmount { get; set; }
}