using System;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.ViewModel.ResellerMarket;

public record GetResellerMarketsRequest : PageableRequest, IRequest<ResellerMarketsResponse>
{
    public string? CurrencyCode { get; init; }

    public string[]? CurrencyCodes { get; init; }

    public DateTimeOffset From { get; init; }

    public Guid ResellerId { get; init; }

    public DateTimeOffset To { get; init; }
}