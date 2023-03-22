using System;
using MediatR;

namespace Tgs.Report.ViewModel.Market;

public record GetMarketAgentProfitsRequest : PageableRequest, IRequest<PagedResponse<MarketAgentProfitModel>>
{
    public Guid BrandId { get; init; }
    public DateTimeOffset Date { get; init; }
}