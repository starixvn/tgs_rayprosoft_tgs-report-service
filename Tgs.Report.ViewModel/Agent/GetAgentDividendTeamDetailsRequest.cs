using System;
using MediatR;

namespace Tgs.Report.ViewModel.Agent
{
    public class GetAgentDividendTeamDetailsRequest : PageableRequest,
        IRequest<PagedResponse<AgentDividendTeamDetailModel>>
    {
        public Guid AgentId { get; set; }

        public DateTimeOffset From { get; set; }

        public DateTimeOffset To { get; set; }
    }
}