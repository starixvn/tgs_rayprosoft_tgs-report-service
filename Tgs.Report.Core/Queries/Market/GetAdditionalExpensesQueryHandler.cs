using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel.Market;

namespace Tgs.Report.Core.Queries.Market
{
    public class
        GetAdditionalExpensesQueryHandler : IRequestHandler<GetAdditionalExpensesRequest, GetAdditionalExpensesResponse>
    {
        private readonly ITgsRepository _repository;

        public GetAdditionalExpensesQueryHandler(ITgsRepository repository)
        {
            _repository = repository;
        }

        public async Task<GetAdditionalExpensesResponse> Handle(
            GetAdditionalExpensesRequest request,
            CancellationToken cancellationToken)
        {
            var playerAggregates = await _repository.PlayerAggregates
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.ReportDate == request.ReportDate)
                .Select(x => new { x.AllAgentRebateAmount }).ToArrayAsync(cancellationToken);

            return new GetAdditionalExpensesResponse
            {
                AdditionalExpenses = new[]
                {
                    new AdditionalExpensesModel
                    {
                        AdditionalExpensesType = AdditionalExpensesType.AllAgent,
                        Amount = playerAggregates.Sum(x => x.AllAgentRebateAmount)
                    }
                }
            };
        }
    }
}