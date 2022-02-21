using System;
using MediatR;

namespace Tgs.Report.ViewModel.Player
{
    public record GetPlayerContributionsRequest : PageableRequest, IRequest<PagedResponse<PlayerContributionModel>>
    {
        public Guid BrandId { get; init; }
        public string? Username { get; init; }
        public int? Level { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}