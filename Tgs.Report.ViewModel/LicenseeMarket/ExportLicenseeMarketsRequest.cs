using System;
using System.IO;
using MediatR;

namespace Tgs.Report.ViewModel.LicenseeMarket
{
    public record ExportLicenseeMarketsRequest : PageableRequest, IRequest<MemoryStream>
    {
        public string CurrencyCode { get; init; } = default!;

        public string[] CurrencyCodes { get; init; } = default!;

        public DateTimeOffset From { get; init; }

        public Guid LicenseeId { get; init; }

        public DateTimeOffset To { get; init; }
    }
}