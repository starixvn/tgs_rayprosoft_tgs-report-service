namespace Tgs.Report.Client.Models.LicenseeMarket
{
    public record LicenseeMarketsResponse
    {
        public PagedResponse<LicenseeMarketDataModel> Datas { get; init; } = default!;
        public LicenseeMarketTotalModel[] Totals { get; init; } = default!;
    }
}