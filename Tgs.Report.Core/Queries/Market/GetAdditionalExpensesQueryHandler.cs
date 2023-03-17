using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Enum;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Market;

namespace Tgs.Report.Core.Queries.Market;

public class
    GetAdditionalExpensesQueryHandler : IRequestHandler<GetAdditionalExpensesRequest, GetAdditionalExpensesResponse>
{
    private readonly TgsContext _context;

    public GetAdditionalExpensesQueryHandler(TgsContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<GetAdditionalExpensesResponse> Handle(
        GetAdditionalExpensesRequest request,
        CancellationToken cancellationToken)
    {
        var playerAggregates = await _context.PlayerAggregates
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate == request.ReportDate.ToUniversalTime())
            .Select(x => new { x.AllAgentRebateAmount }).ToArrayAsync(cancellationToken);

        return new()
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