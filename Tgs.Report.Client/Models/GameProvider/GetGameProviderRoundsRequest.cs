using System;
using Tgs.Report.Enum;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.GameProvider
{
    public record GetGameProviderRoundsRequest : PageableRequest
    {
        public DateTimeOffset From { get; init; }

        public DateTimeOffset To { get; init; }

        public Guid BrandId { get; init; }

        public string GameProviderCode { get; init; }

        public GameType? GameType { get; init; }
    }
}