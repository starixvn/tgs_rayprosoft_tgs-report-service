using MediatR;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent
{
    public class
        GetAgentAccountChangeQueryHandler : IRequestHandler<GetAgentAccountChangesRequest,
            PagedResponse<AgentAccountChangeModel>>
    {
        private readonly ITgsRepository _repository;

        public GetAgentAccountChangeQueryHandler(ITgsRepository repository)
        {
            _repository = repository;
        }

        public Task<PagedResponse<AgentAccountChangeModel>> Handle(
            GetAgentAccountChangesRequest request,
            CancellationToken cancellationToken) =>
            _repository.AgentTransactions
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.AgentId == request.AgentId)
                .Where(request.TransactionType, x => x.Type == (short?)request.TransactionType)
                .Where(x => x.RecordedOn >= request.From)
                .Where(x => x.RecordedOn < request.To)
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