namespace Tgs.Report.Client.Models.AgentWinLoss;

public record ExportAgentWinLossRequest(Guid BrandId, Guid AgentId, decimal PlatformCharge,
    DateTimeOffset From, DateTimeOffset To);