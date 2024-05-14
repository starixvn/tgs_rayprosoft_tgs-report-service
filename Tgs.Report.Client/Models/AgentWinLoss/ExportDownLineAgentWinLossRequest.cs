namespace Tgs.Report.Client.Models.AgentWinLoss;

public record ExportDownLineAgentWinLossRequest(Guid BrandId, Guid AgentId,
    DateTimeOffset From, DateTimeOffset To);