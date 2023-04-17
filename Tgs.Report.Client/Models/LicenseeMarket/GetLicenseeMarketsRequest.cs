namespace Tgs.Report.Client.Models.LicenseeMarket;

public record GetLicenseeMarketsRequest : PageableRequest
{
    public Guid LicenseeId { get; init; }
    public string[]? CurrencyCodes { get; init; }
    public string? CurrencyCode { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}