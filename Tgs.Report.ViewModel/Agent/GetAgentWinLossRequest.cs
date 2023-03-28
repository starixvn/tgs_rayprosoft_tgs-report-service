using System;
using MediatR;

namespace Tgs.Report.ViewModel.Agent;

public record GetAgentWinLossRequest : PageableRequest, IRequest<PagedResponse<AgentWinLossModel>>
{
    public Guid AgentId { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}