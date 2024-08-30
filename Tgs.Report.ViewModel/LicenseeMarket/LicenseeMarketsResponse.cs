using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.ViewModel.LicenseeMarket;

public record LicenseeMarketsResponse
{
    public PagedResponse<LicenseeMarketDataModel>? Datas { get; init; }
    public LicenseeMarketTotalModel[]? Totals { get; init; }
}