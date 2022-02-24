using System;
using MediatR;

namespace Tgs.Report.ViewModel.Round
{
    public record GetClosedRoundsRequest : PageableRequest, IRequest<PagedResponse<ClosedRoundModel>>
    {
        public string? ExternalRoundId { get; init; }

        public Guid? GameProviderId { get; init; }
    }
}