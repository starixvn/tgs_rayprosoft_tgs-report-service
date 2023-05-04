using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core;

public interface IAgentFactory
{
    Task<IEnumerable<AgentContributionDataModel>> GetAgentContributionAsync(GetAgentContributionsDataRequest request);
    //Task<IEnumerable<AgentContributionSubAgentIdDataModel>> GetSubAgentsAsync(GetAgentContributionsDataRequest request);

}
