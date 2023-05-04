using System;

namespace Tgs.Report.ViewModel.Agent;

public record AgentContributionSubAgentIdDataModel
{
    public Guid FirstLayerAgentId { get; init; }
    public Guid AgentId { get; init; }
}