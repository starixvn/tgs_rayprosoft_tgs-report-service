namespace Tgs.Report.Client.Models.Vip;

public record ExportVipReportRequest(Guid BrandId, string? Username, int? Level, DateTimeOffset From,
    DateTimeOffset To);