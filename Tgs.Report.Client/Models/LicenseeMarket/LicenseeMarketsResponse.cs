using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.LicenseeMarket
{
    public record LicenseeMarketsResponse
    {
        public PagedResponse<LicenseeMarketDataModel> Datas { get; init; }
        public LicenseeMarketTotalModel[] Totals { get; init; }
    }
}