using System;
using MediatR;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.GameProvider
{
    public class GetGameProviderRoundsRequest : PageableRequest, IRequest<GetGameProviderRoundsResponse>
    {
        public DateTimeOffset From { get; set; }

        public DateTimeOffset To { get; set; }

        public Guid BrandId { get; set; }

        public string GameProviderCode { get; set; }

        public GameType? GameType { get; set; }
    }
}