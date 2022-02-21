using System;
using MediatR;

namespace Tgs.Report.ViewModel.Agent
{
    public record GetAgentDividendTeamDetailsRequest : PageableRequest,
        IRequest<PagedResponse<AgentDividendTeamDetailModel>>
    {
        public Guid AgentId { get; init; }

        public DateTimeOffset From { get; init; }

        public DateTimeOffset To { get; init; }
    }
}