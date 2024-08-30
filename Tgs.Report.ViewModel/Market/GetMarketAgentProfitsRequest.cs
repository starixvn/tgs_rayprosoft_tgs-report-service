using System;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.ViewModel.Market;

public record GetMarketAgentProfitsRequest : PageableRequest, IRequest<PagedResponse<MarketAgentProfitModel>>
{
    public Guid BrandId { get; init; }
    public DateTimeOffset Date { get; init; }
}