namespace Tgs.Report.Client.Models.Agent;

public record ExportAgentContributionsRequest(Guid BrandId, Guid? AgentId, string Username, short? Type,
    DateTimeOffset From, DateTimeOffset To, bool IncludeAgents);