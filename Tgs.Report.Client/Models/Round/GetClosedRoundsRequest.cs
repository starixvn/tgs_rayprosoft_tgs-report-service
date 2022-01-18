using System;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.Round
{
    public class GetClosedRoundsRequest : PageableRequest
    {
        public string ExternalRoundId { get; set; }

        public Guid? GameProviderId { get; set; }
    }
}