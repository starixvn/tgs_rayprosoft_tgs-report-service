using System;
using MediatR;

namespace Tgs.Report.ViewModel.GameProvider
{
    public record GetGameProviderRoundsRequest : PageableRequest, IRequest<PagedResponse<GameProviderRoundsModel>>
    {
        public DateTimeOffset From { get; init; }

        public DateTimeOffset To { get; init; }

        public Guid GameProviderId { get; init; }
    }
}