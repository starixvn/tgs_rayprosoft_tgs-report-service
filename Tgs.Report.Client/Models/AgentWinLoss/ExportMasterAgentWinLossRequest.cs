namespace Tgs.Report.Client.Models.AgentWinLoss;

public record ExportMasterAgentWinLossRequest(Guid BrandId,
    DateTimeOffset From, DateTimeOffset To);