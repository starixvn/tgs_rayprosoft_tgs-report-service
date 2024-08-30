using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.Client.Models.Player;

public record GetPlayersAccountChangesRequest : PageableRequest
{
    public Guid BrandId { get; init; }
    public string? Username { get; init; }
    public short? TransactionType { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}