using System;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.Round
{
    public class GetOpenRoundsRequest : PageableRequest
    {
        public string ExternalRoundId { get; set; }

        public Guid? GameProviderId { get; set; }

        public DateTimeOffset From { get; set; }

        public DateTimeOffset To { get; set; }
    }
}