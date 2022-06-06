using System;

namespace Tgs.Report.ViewModel.Marketing
{
    public record MarketingModel
    {
        public DateTimeOffset ReportDate { get; init; }

        public int RegisterCount { get; init; }

        public int FirstDepositCount { get; init; }

        public int GamingCount { get; init; }
    }
}