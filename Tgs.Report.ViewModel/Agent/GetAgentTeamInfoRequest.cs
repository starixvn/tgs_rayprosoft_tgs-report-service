using System;
using MediatR;

namespace Tgs.Report.ViewModel.Agent
{
    public record GetAgentTeamInfoRequest : PageableRequest, IRequest<PagedResponse<TeamInfoModel>>
    {
        public Guid AgentId { get; set; }
    }
}