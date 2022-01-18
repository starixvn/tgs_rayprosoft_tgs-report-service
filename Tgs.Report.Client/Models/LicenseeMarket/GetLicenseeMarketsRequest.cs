using System;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.LicenseeMarket
{
    public class GetLicenseeMarketsRequest : PageableRequest
    {
        public Guid LicenseeId { get; set; }
        public string[] CurrencyCodes { get; set; }
        public string CurrencyCode { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}