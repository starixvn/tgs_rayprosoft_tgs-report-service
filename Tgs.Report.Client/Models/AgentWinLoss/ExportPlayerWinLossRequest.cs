namespace Tgs.Report.Client.Models.AgentWinLoss;

public record ExportPlayerWinLossRequest(string PlayerCode, decimal PlatformCharge,
    DateTimeOffset From, DateTimeOffset To);