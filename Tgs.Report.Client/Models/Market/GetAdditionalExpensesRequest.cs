using System;

namespace Tgs.Report.Client.Models.Market
{
    public class GetAdditionalExpensesRequest
    {
        public Guid BrandId { get; set; }

        public DateTimeOffset ReportDate { get; set; }
    }
}