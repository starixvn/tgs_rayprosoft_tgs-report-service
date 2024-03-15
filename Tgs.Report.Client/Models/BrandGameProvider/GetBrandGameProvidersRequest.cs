using GameType = Tgs.Report.Client.Enum.GameType;

namespace Tgs.Report.Client.Models.BrandGameProvider;

public record GetBrandGameProvidersRequest : PageableRequest
{
    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }

    public Guid BrandId { get; init; }

    public string? GameProviderCode { get; init; }

    public GameType? GameType { get; init; }
}