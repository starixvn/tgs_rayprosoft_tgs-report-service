using Tgs.Report.Enum;

namespace Tgs.Report.Core.Models;

public record GetPlayerAccountChangesTotalAmountQuery
{
    public Guid BrandId { get; init; }
    public string PlayerCode { get; init; } = default!;
    public AccountChangeType? AccountChangeType { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}