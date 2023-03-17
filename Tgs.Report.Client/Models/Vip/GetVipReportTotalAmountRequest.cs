namespace Tgs.Report.Client.Models.Vip;

public record GetVipReportTotalAmountRequest
{
    public Guid BrandId { get; init; }
    public string? Username { get; init; }
    public int? Level { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}