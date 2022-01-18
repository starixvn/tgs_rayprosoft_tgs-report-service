namespace Tgs.Report.ViewModel.LicenseeMarket
{
    public class LicenseeMarketsResponse
    {
        public PagedResponse<LicenseeMarketDataModel> Datas { get; set; }
        public LicenseeMarketTotalModel[] Totals { get; set; }
    }
}