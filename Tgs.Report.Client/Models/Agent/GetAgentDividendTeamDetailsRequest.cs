using System;

namespace Tgs.Report.Client.Models.Agent
{
    public record GetAgentDividendTeamDetailsRequest : PageableRequest
    {
        public Guid AgentId { get; init; }
        public decimal AgentDividendPercentage { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}