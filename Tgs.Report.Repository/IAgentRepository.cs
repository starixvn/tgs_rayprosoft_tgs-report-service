using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tgs.Report.Repository;

public interface IAgentRepository
{
    Task<AgentWithNLevelInfo?> GetAgentWithNLevelAsync(
        Guid brandId,
        Guid? agentId,
        string? userName,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<AgentInfo> GetFirstLayerAgentsAsync(Guid brandId, Guid parentAgentId);

    IAsyncEnumerable<ChildAgentInfo> GetChildAgentsAsync(Guid brandId, AgentWithNLevelInfo parentAgentInfo);
}