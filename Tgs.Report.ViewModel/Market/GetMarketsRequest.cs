using System;
using MediatR;

namespace Tgs.Report.ViewModel.Market;

public record GetMarketsRequest : PageableRequest, IRequest<PagedResponse<MarketModel>>
{
    public Guid BrandId { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}