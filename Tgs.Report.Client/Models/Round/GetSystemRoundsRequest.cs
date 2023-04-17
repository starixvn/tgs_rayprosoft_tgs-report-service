namespace Tgs.Report.Client.Models.Round;

public record GetSystemRoundsRequest : PageableRequest
{
    public string? Username { get; init; }
    public string? PlayerCode { get; init; }
    public string? ExternalRoundId { get; init; }
    public Guid? RoundId { get; init; }
    public DateTimeOffset? ExternalFrom { get; init; }
    public DateTimeOffset? ExternalTo { get; init; }
    public DateTimeOffset? ExternalClosedFrom { get; init; }
    public DateTimeOffset? ExternalClosedTo { get; init; }
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
    public DateTimeOffset? ClosedFrom { get; init; }
    public DateTimeOffset? ClosedTo { get; init; }
    public short? Status { get; init; }
    public Guid? BrandId { get; init; }
    public Guid GameProviderId { get; init; }
    public string? SubGameProviderCode { get; init; }
    public short? GameType { get; init; }
    public string? CurrencyCode { get; init; }
}