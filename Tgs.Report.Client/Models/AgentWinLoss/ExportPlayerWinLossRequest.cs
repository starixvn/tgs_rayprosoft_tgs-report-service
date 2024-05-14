namespace Tgs.Report.Client.Models.AgentWinLoss;

public record ExportPlayerWinLossRequest(string PlayerCode,
    DateTimeOffset From, DateTimeOffset To);