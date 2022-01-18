using System;
using MediatR;

namespace Tgs.Report.ViewModel.Agent
{
    public class GetAgentTeamInfoRequest : PageableRequest, IRequest<PagedResponse<TeamInfoModel>>
    {
        public Guid AgentId { get; set; }
    }
}