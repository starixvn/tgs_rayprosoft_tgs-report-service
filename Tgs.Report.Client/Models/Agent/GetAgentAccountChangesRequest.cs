using System;
using Tgs.Report.Enum;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.Agent
{
    public class GetAgentAccountChangesRequest : PageableRequest
    {
        public Guid BrandId { get; set; }
        public Guid AgentId { get; set; }
        public TransactionType? TransactionType { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}