using System;
using MediatR;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Agent
{
    public record GetAgentAccountChangesRequest : PageableRequest, IRequest<PagedResponse<AgentAccountChangeModel>>
    {
        public Guid BrandId { get; set; }
        public Guid AgentId { get; set; }
        public TransactionType? TransactionType { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}