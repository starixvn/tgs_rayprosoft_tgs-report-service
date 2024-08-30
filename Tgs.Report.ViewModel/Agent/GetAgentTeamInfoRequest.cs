using System;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.ViewModel.Agent;

public record GetAgentTeamInfoRequest : PageableRequest, IRequest<PagedResponse<TeamInfoModel>>
{
    public Guid AgentId { get; set; }
}