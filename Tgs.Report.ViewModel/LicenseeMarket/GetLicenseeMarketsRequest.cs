using System;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.ViewModel.LicenseeMarket;

public record GetLicenseeMarketsRequest : PageableRequest, IRequest<LicenseeMarketsResponse>
{
    public string? CurrencyCode { get; init; }

    public string[]? CurrencyCodes { get; init; }

    public DateTimeOffset From { get; init; }

    public Guid LicenseeId { get; init; }

    public DateTimeOffset To { get; init; }
}