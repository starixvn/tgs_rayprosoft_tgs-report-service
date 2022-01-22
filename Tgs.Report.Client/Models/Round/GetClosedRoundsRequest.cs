using System;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.Round
{
    public record GetClosedRoundsRequest : PageableRequest
    {
        public string ExternalRoundId { get; init; }

        public Guid? GameProviderId { get; init; }
    }
}