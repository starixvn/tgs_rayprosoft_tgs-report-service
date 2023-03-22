namespace Tgs.Report.Client.Models.Player;

public record GetPlayerAccountChangesTotalRequest
{
    public Guid BrandId { get; init; }
    public string PlayerCode { get; init; } = default!;
    public short? AccountChangeType { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}