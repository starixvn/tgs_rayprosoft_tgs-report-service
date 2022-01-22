using System;
using MediatR;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.GameProvider
{
    public record GetGameProviderRoundsRequest : PageableRequest, IRequest<GetGameProviderRoundsResponse>
    {
        public DateTimeOffset From { get; init; }

        public DateTimeOffset To { get; init; }

        public Guid BrandId { get; init; }

        public string GameProviderCode { get; init; } = default!;

        public GameType? GameType { get; init; }
    }
}