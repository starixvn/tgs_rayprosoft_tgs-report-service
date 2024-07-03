
namespace Tgs.Report.Core.Models
{
    public record GetPlayerAllAgentRebatesQuery(
        Guid BrandId,
        DateTime From,
        DateTime To,
        string? Username,
        string? PlayerCode);
}
