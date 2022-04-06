using System;

namespace Tgs.Report.Client.Models.ResellerMarket
{
    public record GetResellerMarketsRequest : PageableRequest
    {
        public string? CurrencyCode { get; init; }

        public string[]? CurrencyCodes { get; init; }

        public DateTimeOffset From { get; init; }

        public Guid ResellerId { get; init; }

        public DateTimeOffset To { get; init; }
    }
}