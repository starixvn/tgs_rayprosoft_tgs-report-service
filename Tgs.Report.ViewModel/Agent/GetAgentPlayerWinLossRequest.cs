using System;
using MediatR;

namespace Tgs.Report.ViewModel.Agent
{
    public class GetAgentPlayerWinLossRequest : PageableRequest, IRequest<PagedResponse<AgentPlayerWinLossModel>>
    {
        public Guid AgentId { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}