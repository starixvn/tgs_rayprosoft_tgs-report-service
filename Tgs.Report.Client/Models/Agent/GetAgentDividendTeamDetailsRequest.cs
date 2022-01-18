using System;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.Agent
{
    public class GetAgentDividendTeamDetailsRequest : PageableRequest
    {
        public Guid AgentId { get; set; }

        public DateTimeOffset From { get; set; }

        public DateTimeOffset To { get; set; }
    }
}