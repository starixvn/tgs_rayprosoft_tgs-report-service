using Tgs.Report.ViewModel.Market;

namespace Tgs.Report.Core;

public interface IMarketFactory
{
    Task<IEnumerable<MarketAgentProfitDataModel>> GetAgentWinLossAsync(GetMarketAgentProfitsRequest request);
}
