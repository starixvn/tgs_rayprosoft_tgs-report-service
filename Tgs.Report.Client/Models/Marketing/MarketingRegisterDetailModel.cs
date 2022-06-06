namespace Tgs.Report.Client.Models.Marketing
{
    public record MarketingRegisterDetailModel
    {
        public string Username { get; init; } = default!;
        public DateTimeOffset RegisteredOn { get; init; }
        public DateTimeOffset LastLoginOn { get; init; }
        public int LoginCount { get; init; }
    }
}