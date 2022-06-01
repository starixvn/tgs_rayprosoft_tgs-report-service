namespace Tgs.Report.Client.Models.Market
{
    public record GetPlayerNoSecondDepositDetailsRequest : PageableRequest
    {
        public Guid BrandId { get; init; }
        public DateTimeOffset Date { get; init; }
    }
}