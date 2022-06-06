namespace Tgs.Report.Client.Models.Marketing
{
    public record GetMarketingFirstDepositDetailRequest : PageableRequest
    {
        public Guid BrandId { get; init; }
        public string AgentUsername { get; init; } = default!;
        public DateTimeOffset Date { get; init; }
    }
}