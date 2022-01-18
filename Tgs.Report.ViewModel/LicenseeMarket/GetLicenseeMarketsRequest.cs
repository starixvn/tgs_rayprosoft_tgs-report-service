using System;
using MediatR;

namespace Tgs.Report.ViewModel.LicenseeMarket
{
    public class GetLicenseeMarketsRequest : PageableRequest, IRequest<LicenseeMarketsResponse>
    {
        public string CurrencyCode { get; set; }

        public string[]? CurrencyCodes { get; set; }

        public DateTimeOffset From { get; set; }

        public Guid LicenseeId { get; set; }

        public DateTimeOffset To { get; set; }
    }
}