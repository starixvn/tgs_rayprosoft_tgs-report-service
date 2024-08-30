using System;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.ViewModel.AgentWinLoss;

public record GetPlayerWinLossRequest : PageableRequest, IRequest<PagedResponse<AgentWinLossModel>>
{
    public string PlayerCode { get; init; } = default!;
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}