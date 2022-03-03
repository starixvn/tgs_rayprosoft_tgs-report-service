using System;

namespace Tgs.Report.Client.Models.GameProvider
{
    public record GetGameProviderRoundsRequest : PageableRequest
    {
        public DateTimeOffset From { get; init; }

        public DateTimeOffset To { get; init; }

        public Guid? GameProviderId { get; init; }
    }
}