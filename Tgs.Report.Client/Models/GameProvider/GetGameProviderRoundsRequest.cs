using System;
using Tgs.Report.Enum;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.GameProvider
{
    public class GetGameProviderRoundsRequest : PageableRequest
    {
        public DateTimeOffset From { get; set; }

        public DateTimeOffset To { get; set; }

        public Guid BrandId { get; set; }

        public string GameProviderCode { get; set; }

        public GameType? GameType { get; set; }
    }
}