using System;
using MediatR;

namespace Tgs.Report.ViewModel.Player;

public record GetPlayerContributionsTotalAmountRequest : IRequest<PlayerContributionTotalAmountModel>
{
    public Guid BrandId { get; init; }
    public string? Username { get; init; }
    public int? Level { get; init; }

    public bool? HasUpperAgent { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}