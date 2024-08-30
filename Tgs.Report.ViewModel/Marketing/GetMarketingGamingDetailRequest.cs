using System;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.ViewModel.Marketing;

public record GetMarketingGamingDetailRequest : PageableRequest, IRequest<PagedResponse<MarketingGamingDetailModel>>
{
    public Guid BrandId { get; init; }
    public string AgentUsername { get; init; } = default!;
    public string? PlayerUsername { get; init; }
    public DateTimeOffset Date { get; init; }
}