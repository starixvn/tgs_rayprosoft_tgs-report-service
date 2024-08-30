using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.Client.Models.ResellerMarket;

public record ResellerMarketsResponse
{
    public PagedResponse<ResellerMarketDataModel>? Datas { get; init; }
    public ResellerMarketTotalModel[]? Totals { get; init; }
}