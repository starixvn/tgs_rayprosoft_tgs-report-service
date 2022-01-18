using System;
using Tgs.Report.Enum;

namespace Tgs.Report.Client.Models.Agent
{
    public record ExportAgentContributionsRequest(Guid BrandId, Guid? AgentId, string Username, AgentType? Type,
        DateTimeOffset From, DateTimeOffset To, bool IncludeAgents);
}