namespace Tgs.Report.Client.Models.AgentWinLoss;

public record ExportMasterAgentWinLossRequest(Guid BrandId, decimal PlatformCharge,
    DateTimeOffset From, DateTimeOffset To);