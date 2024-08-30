using System;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.ViewModel.Agent;

public record GetAgentDividendDetailsRequest : PageableRequest, IRequest<PagedResponse<AgentDividendDetailModel>>
{
    public Guid AgentId { get; init; }
    public decimal AgentDividendPercentage { get; set; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}