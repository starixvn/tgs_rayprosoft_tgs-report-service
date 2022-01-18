using System;
using MediatR;

namespace Tgs.Report.ViewModel.Round
{
    public class GetOpenRoundsRequest : PageableRequest, IRequest<PagedResponse<OpenRoundModel>>
    {
        public string ExternalRoundId { get; set; }

        public Guid? GameProviderId { get; set; }

        public DateTimeOffset From { get; set; }

        public DateTimeOffset To { get; set; }
    }
}