using System;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.Agent
{
    public class GetAgentTeamInfoRequest : PageableRequest
    {
        public Guid AgentId { get; set; }
    }
}