namespace Tgs.Report.Client.Models.Round;

public record ExportRoundsRequest(
    Guid BrandId,
    string? Username,
    string? PlayerCode,
    Guid? RoundId,
    string? ExternalRoundId,
    Guid? GameProviderId,
    short? Status,
    short? GameType,
    DateTimeOffset? From,
    DateTimeOffset? To,
    DateTimeOffset? ClosedFrom,
    DateTimeOffset? ClosedTo);