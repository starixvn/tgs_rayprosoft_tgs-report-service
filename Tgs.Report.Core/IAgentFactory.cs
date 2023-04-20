using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core;

public interface IAgentFactory
{
    Task<IEnumerable<AgentWinLossDataModel>> GetMasterAgentWinLossAsync(GetMasterAgentWinLossDataRequest request);
    Task<IEnumerable<AgentWinLossDataModel>> GetAgentWinLossAsync(GetAgentWinLossDataRequest request);
    Task<IEnumerable<AgentWinLossDataModel>> GetDownLineAgentWinLossAsync(GetDownLineAgentWinLossDataRequest request);
}