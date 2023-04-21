using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core;

public interface IAgentFactory
{
    Task<IEnumerable<AgentWinLossModel>> GetMasterAgentWinLossAsync(GetMasterAgentWinLossDataRequest request);
    Task<IEnumerable<AgentWinLossModel>> GetAgentWinLossAsync(GetAgentWinLossDataRequest request);
    Task<IEnumerable<AgentWinLossModel>> GetDownLineAgentWinLossAsync(GetDownLineAgentWinLossDataRequest request);
}