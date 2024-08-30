using System;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.ViewModel.AgentWinLoss;

public record GetMasterAgentWinLossRequest : PageableRequest, IRequest<PagedResponse<AgentWinLossModel>>
{
    public Guid BrandId { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}