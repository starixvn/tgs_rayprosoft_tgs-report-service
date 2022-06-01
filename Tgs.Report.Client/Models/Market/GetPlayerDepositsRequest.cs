namespace Tgs.Report.Client.Models.Market
{
    public record GetPlayerDepositsRequest : PageableRequest
    {
        public Guid BrandId { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}