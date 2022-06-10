namespace Tgs.Report.Client.Models.Marketing
{
    public record GetMarketingGamingDetailRequest : PageableRequest
    {
        public Guid BrandId { get; init; }
        public string AgentUsername { get; init; } = default!;
        public string? PlayerUsername { get; init; }
        public DateTimeOffset Date { get; init; }
    }
}