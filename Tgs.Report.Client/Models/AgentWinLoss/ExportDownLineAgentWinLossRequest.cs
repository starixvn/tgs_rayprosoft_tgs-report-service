namespace Tgs.Report.Client.Models.AgentWinLoss;

public record ExportDownLineAgentWinLossRequest(Guid BrandId, Guid AgentId, decimal PlatformCharge,
    DateTimeOffset From, DateTimeOffset To);