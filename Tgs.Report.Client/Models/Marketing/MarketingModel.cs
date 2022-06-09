namespace Tgs.Report.Client.Models.Marketing
{
    public record MarketingModel
    {
        public DateTimeOffset ReportDate { get; init; }

        public int RegisterCount { get; init; }

        public int FirstDepositCount { get; init; }

        public int GamingCount { get; init; }
    }
}