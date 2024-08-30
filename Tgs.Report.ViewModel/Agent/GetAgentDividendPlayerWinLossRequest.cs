using System;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.ViewModel.Agent;

public record GetAgentDividendPlayerWinLossRequest : PageableRequest, IRequest<PagedResponse<AgentPlayerWinLossModel>>
{
    public Guid AgentId { get; init; }
    public decimal AgentDividendPercentage { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}