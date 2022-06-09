using MediatR;
using Tgs.Report.Enum;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent
{
    public class
        GetAgentAccountChangeQueryHandler : IRequestHandler<GetAgentAccountChangesRequest,
            PagedResponse<AgentAccountChangeModel>>
    {
        private readonly TgsContext _context;

        public GetAgentAccountChangeQueryHandler(TgsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<PagedResponse<AgentAccountChangeModel>> Handle(
            GetAgentAccountChangesRequest request,
            CancellationToken cancellationToken)
            => _context.AgentTransactions
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.AgentId == request.AgentId)
                .Where(request.TransactionType, x => x.Type == (short?)request.TransactionType)
                .Where(x => x.RecordedOn >= request.From.ToUniversalTime())
                .Where(x => x.RecordedOn < request.To.ToUniversalTime())
                .Select(x => new AgentAccountChangeModel
                {
                    TransactionId = x.Id,
                    Amount = x.Amount,
                    Balance = x.Balance,
                    TransactionType = (TransactionType)x.Type,
                    RecordedOn = x.RecordedOn
                }).CreatePagedResponseAsync(request);
    }
}