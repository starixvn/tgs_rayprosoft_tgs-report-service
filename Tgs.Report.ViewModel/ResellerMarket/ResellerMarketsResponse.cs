namespace Tgs.Report.ViewModel.ResellerMarket;

public record ResellerMarketsResponse
{
    public PagedResponse<ResellerMarketDataModel>? Datas { get; init; }
    public ResellerMarketTotalModel[]? Totals { get; init; }
}