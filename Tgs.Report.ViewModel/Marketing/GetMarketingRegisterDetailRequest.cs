using System;
using MediatR;

namespace Tgs.Report.ViewModel.Marketing;

public record GetMarketingRegisterDetailRequest : PageableRequest, IRequest<PagedResponse<MarketingRegisterDetailModel>>
{
    public Guid BrandId { get; init; }
    public string AgentUsername { get; init; } = default!;
    public DateTimeOffset Date { get; init; }
}