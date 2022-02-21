using System;

namespace Tgs.Report.Client.Models.Agent
{
    public record ExportAgentDividendsRequest(Guid BrandId, Guid? AgentId, string Username, short? Type,
        DateTimeOffset From, DateTimeOffset To);
}