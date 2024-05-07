using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core;

public interface IAgentFactory
{
    Task<IEnumerable<AgentWinLossModel>> GetMasterAgentWinLossAsync(GetMasterAgentWinLossDataRequest request);
    Task<IEnumerable<AgentWinLossModel>> GetAgentWinLossAsync(GetAgentWinLossDataRequest request);
    Task<IEnumerable<AgentWinLossModel>> GetDownLineAgentWinLossAsync(GetDownLineAgentWinLossDataRequest request);
    Task<AgentPlayerSpecialFeesModel> GetAgentPlayerSpecialFeesAsync(GetAgentPlayerSpecialFeesDataRequest request);
    Task<IEnumerable<SpecialDetailDataModel>> GetSpecialDetailAsync(GetSpecialDetailDataRequest request);
    Task<decimal> GetAgentPlayerSpecialProductFeeAsync(GetAgentPlayerSpecialProductFeeDataRequest request);
}