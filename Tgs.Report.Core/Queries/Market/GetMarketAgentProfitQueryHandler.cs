using MediatR;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Market;

namespace Tgs.Report.Core.Queries.Market;

public class
    GetMarketAgentProfitQueryHandler : IRequestHandler<GetMarketAgentProfitsRequest,
        PagedResponse<MarketAgentProfitModel>>
{
    private readonly IMarketFactory _marketFactory;

    public GetMarketAgentProfitQueryHandler(IMarketFactory marketFactory)
    {
        _marketFactory = marketFactory ?? throw new ArgumentNullException(nameof(marketFactory));
    }

    public async Task<PagedResponse<MarketAgentProfitModel>> Handle(
        GetMarketAgentProfitsRequest request,
        CancellationToken cancellationToken)
    {
        var offset = new TimeSpan(8, 0, 0);
        var requestFrom = request.Date.ToOffset(offset);

        var from = new DateTimeOffset(requestFrom.Year,
            requestFrom.Month,
            requestFrom.Day,
            0,
            0,
            0,
            requestFrom.Offset);

        var list = await _marketFactory.GetAgentWinLossAsync(request);

        return new PagedResponse<MarketAgentProfitModel>
        {
            Valid = true,
            TotalCount = list.FirstOrDefault().TotalCount,
            Items = list.Select(x => new MarketAgentProfitModel
            {
                MasterAgentName = x.MasterAgentName,
                Percentage = x.Percentage,
                DirectPlayerNetProfit = x.DirectPlayerNetProfit,
                TeamNetProfit = x.TeamNetProfit,
                CompanyAdjustment = x.CompanyAdjustment,
                AgentProfit = x.AgentProfit,
                CompanyProfit = x.CompanyProfit
            }).ToArray()
        };
    }
}