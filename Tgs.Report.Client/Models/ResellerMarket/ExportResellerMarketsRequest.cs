namespace Tgs.Report.Client.Models.ResellerMarket;

public record ExportResellerMarketsRequest : PageableRequest
{
    public string? CurrencyCode { get; init; }

    public string[]? CurrencyCodes { get; init; }

    public DateTimeOffset From { get; init; }

    public Guid LicenseeId { get; init; }

    public DateTimeOffset To { get; init; }
}