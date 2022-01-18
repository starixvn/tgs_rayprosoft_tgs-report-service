using System;
using System.IO;
using MediatR;

namespace Tgs.Report.ViewModel.LicenseeMarket
{
    public class ExportLicenseeMarketsRequest : PageableRequest, IRequest<MemoryStream>
    {
        public string CurrencyCode { get; set; }

        public string[] CurrencyCodes { get; set; }

        public DateTimeOffset From { get; set; }

        public Guid LicenseeId { get; set; }

        public DateTimeOffset To { get; set; }
    }
}