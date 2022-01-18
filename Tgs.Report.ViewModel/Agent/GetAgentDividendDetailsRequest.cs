using System;
using MediatR;

namespace Tgs.Report.ViewModel.Agent
{
    public class GetAgentDividendDetailsRequest : PageableRequest, IRequest<PagedResponse<AgentDividendDetailModel>>
    {
        public Guid AgentId { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}