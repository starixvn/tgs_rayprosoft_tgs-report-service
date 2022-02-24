using System;
using System.IO;
using MediatR;

namespace Tgs.Report.ViewModel.LicenseeMarket
{
    public record ExportLicenseeMarketsRequest : PageableRequest, IRequest<MemoryStream>
    {
        public string? CurrencyCode { get; init; }

        public string[]? CurrencyCodes { get; init; }

        public DateTimeOffset From { get; init; }

        public Guid LicenseeId { get; init; }

        public DateTimeOffset To { get; init; }
    }
}