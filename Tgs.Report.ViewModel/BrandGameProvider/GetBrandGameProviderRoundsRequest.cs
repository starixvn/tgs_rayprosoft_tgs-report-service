using System;
using MediatR;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.BrandGameProvider;

public record GetBrandGameProviderRoundsRequest : PageableRequest, IRequest<GetBrandGameProviderRoundsResponse>
{
    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }

    public Guid BrandId { get; init; }

    public string? GameProviderCode { get; init; }

    public GameType? GameType { get; init; }
}