using System;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Round;

public record GetRoundsRequest : PageableRequest, IRequest<PagedResponse<RoundModel>>
{
    public Guid BrandId { get; init; }
    public string? Username { get; init; }
    public string? PlayerCode { get; init; }
    public Guid? RoundId { get; init; }
    public string? ExternalRoundId { get; init; }
    public Guid? GameProviderId { get; init; }
    public ClientRoundStatus? Status { get; init; }
    public GameType? GameType { get; init; }
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
    public DateTimeOffset? ClosedFrom { get; init; }
    public DateTimeOffset? ClosedTo { get; init; }
}