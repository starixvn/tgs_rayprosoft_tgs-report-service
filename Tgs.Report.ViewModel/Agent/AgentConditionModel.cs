using System;

namespace Tgs.Report.ViewModel.Agent;

public record AgentConditionModel
{
    public Guid AgentId { get; init; }
    public decimal ActiveCount { get; init; }
    public decimal NetProfit { get; init; }
    public decimal DividendPercentage { get; init; }
}