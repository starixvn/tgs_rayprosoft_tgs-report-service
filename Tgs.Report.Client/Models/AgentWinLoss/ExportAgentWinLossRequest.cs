namespace Tgs.Report.Client.Models.AgentWinLoss;

public record ExportAgentWinLossRequest(Guid BrandId, Guid AgentId,
    DateTimeOffset From, DateTimeOffset To);