using System;
using MediatR;

namespace Tgs.Report.ViewModel.Agent;

public record GetDownLineAgentWinLossRequest : PageableRequest, IRequest<PagedResponse<AgentWinLossModel>>
{
    public Guid BrandId { get; init; }
    public Guid AgentId { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}