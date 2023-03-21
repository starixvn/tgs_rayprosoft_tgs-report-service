using System;

namespace Tgs.Report.Repository.Entities;

public class AgentDividendPercentage
{
    public Guid AgentId { get; set; }
    public Guid? ParentAgentId { get; set; }
    public decimal DividendPercentage { get; set; }
}