namespace Tgs.Report.Client.Models.PlayerDeposit
{
    public record GetPlayerNoSecondDepositDetailsRequest : PageableRequest
    {
        public Guid BrandId { get; init; }
        public DateTimeOffset Date { get; init; }
    }
}