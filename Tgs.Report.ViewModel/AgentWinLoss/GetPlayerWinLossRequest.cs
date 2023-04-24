using System;
using MediatR;

namespace Tgs.Report.ViewModel.AgentWinLoss;

public record GetPlayerWinLossRequest : PageableRequest, IRequest<PagedResponse<AgentWinLossModel>>
{
    public string PlayerCode { get; init; } = default!;
    public decimal PlatformCharge { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}