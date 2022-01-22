using System;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.LicenseeMarket
{
    public record ExportLicenseeMarketsRequest : PageableRequest
    {
        public Guid LicenseeId { get; init; }
        public string[] CurrencyCodes { get; init; }
        public string CurrencyCode { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}