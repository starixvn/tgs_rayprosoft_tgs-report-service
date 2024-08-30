using TGS.Core.Infrastructures.Extensions.Models;
using Tgs.Report.Enum;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Core.Models;

public record GetPlayerAccountChangesQuery : PageableRequest
{
    public Guid BrandId { get; init; }
    public string PlayerCode { get; init; } = default!;
    public AccountChangeType? AccountChangeType { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}