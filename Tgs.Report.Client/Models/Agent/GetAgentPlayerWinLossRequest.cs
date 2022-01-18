using System;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.Agent
{
    public class GetAgentPlayerWinLossRequest : PageableRequest
    {
        public Guid AgentId { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}