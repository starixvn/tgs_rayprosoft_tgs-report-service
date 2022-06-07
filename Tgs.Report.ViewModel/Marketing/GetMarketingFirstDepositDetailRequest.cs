using System;
using MediatR;

namespace Tgs.Report.ViewModel.Marketing
{
    public record GetMarketingFirstDepositDetailRequest : PageableRequest, IRequest<PagedResponse<MarketingFirstDepositDetailModel>>
    {
        public Guid BrandId { get; init; }
        public string AgentUsername { get; init; } = default!;
        public DateTimeOffset Date { get; init; }
    }
}