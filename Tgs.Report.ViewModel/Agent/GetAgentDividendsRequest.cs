using System;
using MediatR;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Agent
{
    public class GetAgentDividendsRequest : PageableRequest, IRequest<PagedResponse<AgentDividendModel>>
    {
        public Guid BrandId { get; set; }
        public Guid? AgentId { get; set; }
        public string Username { get; set; }
        public AgentType? Type { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}