namespace Tgs.Report.Client.Models.PlayerDeposit
{
    public record GetPlayerDepositsRequest : PageableRequest
    {
        public Guid BrandId { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}