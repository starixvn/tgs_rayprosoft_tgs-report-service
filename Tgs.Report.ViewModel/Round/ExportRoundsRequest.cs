using System;
using System.IO;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Round;

public record ExportRoundsRequest(
    Guid BrandId,
    string? Username,
    string? PlayerCode,
    Guid? RoundId,
    string? ExternalRoundId,
    Guid? GameProviderId,
    ClientRoundStatus? Status,
    GameType? GameType,
    DateTimeOffset? From,
    DateTimeOffset? To,
    DateTimeOffset? ClosedFrom,
    DateTimeOffset? ClosedTo) : PageableRequest,IRequest<MemoryStream>;