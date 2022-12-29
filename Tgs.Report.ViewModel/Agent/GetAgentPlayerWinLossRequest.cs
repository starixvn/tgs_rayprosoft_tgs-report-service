using System;
using MediatR;

namespace Tgs.Report.ViewModel.Agent
{
    public record GetAgentPlayerWinLossRequest : PageableRequest, IRequest<PagedResponse<AgentPlayerWinLossModel>>
    {
        public Guid AgentId { get; init; }
        public decimal AgentDividendPercentage { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}