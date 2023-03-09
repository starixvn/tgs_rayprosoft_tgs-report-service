using System;
using MediatR;

namespace Tgs.Report.ViewModel.Market
{
    public record GetMarketTotalAgentDividendsRequest : PageableRequest, IRequest<PagedResponse<MarketTotalAgentDividendModel>>
    {
        public Guid BrandId { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}