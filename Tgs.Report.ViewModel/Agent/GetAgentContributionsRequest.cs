using System;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Agent;

public record GetAgentContributionsRequest : PageableRequest, IRequest<PagedResponse<AgentContributionModel>>
{
    public Guid BrandId { get; set; }
    public Guid? AgentId { get; set; }
    public string? Username { get; set; }
    public AgentType? Type { get; set; }
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
    public bool IncludeAgents { get; set; }
}