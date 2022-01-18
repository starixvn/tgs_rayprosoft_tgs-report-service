using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.LicenseeMarket
{
    public class LicenseeMarketsResponse
    {
        public PagedResponse<LicenseeMarketDataModel> Datas { get; set; }
        public LicenseeMarketTotalModel[] Totals { get; set; }
    }
}