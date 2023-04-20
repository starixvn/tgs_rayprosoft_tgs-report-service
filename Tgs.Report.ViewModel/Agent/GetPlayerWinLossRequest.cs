using System;
using MediatR;

namespace Tgs.Report.ViewModel.Agent;

public record GetPlayerWinLossRequest : PageableRequest, IRequest<PagedResponse<AgentWinLossModel>>
{
    public string PlayerCode { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}