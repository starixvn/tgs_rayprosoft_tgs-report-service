using System;
using Tgs.Report.Enum;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.Agent
{
    public record GetAgentDividendsRequest : PageableRequest
    {
        public Guid BrandId { get; init; }
        public Guid? AgentId { get; init; }
        public string Username { get; init; }
        public AgentType? Type { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}