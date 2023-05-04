using System;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Agent;

public record AgentContributionDataModel
{
    public Guid MasterId { get; init; }
    public Guid AgentId { get; init; }
    public string Username { get; init; } = default!;
    public decimal CompanyAdjustAmount { get; init; }
    public decimal AgentAdjustAmount { get; init; }
    public decimal BonusAdjustAmount { get; init; }
    public decimal BonusAmount { get; init; }
    public decimal BetAmount { get; init; }
    public decimal OriginalTurnover { get; init; }
    public decimal Turnover { get; init; }
    public decimal WinLossAmount { get; init; }
    public decimal NetProfit { get; init; }
}