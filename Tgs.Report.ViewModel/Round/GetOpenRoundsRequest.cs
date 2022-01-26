using System;
using MediatR;

namespace Tgs.Report.ViewModel.Round
{
    public record GetOpenRoundsRequest : PageableRequest, IRequest<PagedResponse<OpenRoundModel>>
    {
        public string? ExternalRoundId { get; init; }

        public Guid? GameProviderId { get; init; }

        public DateTimeOffset From { get; init; }

        public DateTimeOffset To { get; init; }
    }
}