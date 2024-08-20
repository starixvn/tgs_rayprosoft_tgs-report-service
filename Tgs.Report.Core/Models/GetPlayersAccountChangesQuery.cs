
using Tgs.Report.Enum;

namespace Tgs.Report.Core.Models
{
    public record GetPlayersAccountChangesQuery(
        Guid BrandId, 
        DateTimeOffset From, 
        DateTimeOffset To, 
        string? Username, 
        TransactionType? TransactionType);
}
