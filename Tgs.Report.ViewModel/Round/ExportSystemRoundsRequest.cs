using System;
using System.IO;
using MediatR;

namespace Tgs.Report.ViewModel.Round;

public record ExportSystemRoundsRequest(
    string? Username,
    string? PlayerCode,
    string? ExternalRoundId,
    Guid? RoundId,
    DateTimeOffset? ExternalFrom,
    DateTimeOffset? ExternalTo,
    DateTimeOffset? ExternalClosedFrom,
    DateTimeOffset? ExternalClosedTo,
    DateTimeOffset? From,
    DateTimeOffset? To,
    DateTimeOffset? ClosedFrom,
    DateTimeOffset? ClosedTo,
    short? Status,
    Guid? BrandId,
    Guid GameProviderId,
    string? SubGameProviderCode,
    short? GameType,
    string? CurrencyCode) : IRequest<MemoryStream>;