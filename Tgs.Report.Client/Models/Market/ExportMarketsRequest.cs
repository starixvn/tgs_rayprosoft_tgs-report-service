using System;

namespace Tgs.Report.Client.Models.Market
{
    public record ExportMarketsRequest(Guid BrandId, DateTimeOffset From, DateTimeOffset To);
}