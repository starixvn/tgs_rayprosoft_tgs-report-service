using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.Client.Models.BrandGameProvider;

public record GetBrandGameProviderRoundsRequest : PageableRequest
{
    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }

    public Guid BrandId { get; init; }

    public string? GameProviderCode { get; init; }

    public short? GameType { get; init; }
}