using System;
using Tgs.Report.Enum;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.Agent
{
    public record GetAgentAccountChangesRequest : PageableRequest
    {
        public Guid BrandId { get; init; }
        public Guid AgentId { get; init; }
        public TransactionType? TransactionType { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}