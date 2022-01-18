using System;
using MediatR;

namespace Tgs.Report.ViewModel.Market
{
    public class GetMarketsRequest : PageableRequest, IRequest<PagedResponse<MarketModel>>
    {
        public Guid BrandId { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}