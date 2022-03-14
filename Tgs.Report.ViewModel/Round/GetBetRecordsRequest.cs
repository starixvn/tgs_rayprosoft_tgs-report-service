using System;
using MediatR;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Round
{
    public record GetBetRecordsRequest : PageableRequest, IRequest<GetBetRecordsResponse>
    {
        public Guid BrandId { get; init; }
        public string PlayerCode { get; init; } = default!;
        public Guid? GameProviderId { get; init; }
        public ClientRoundStatus? Status { get; init; }
        public GameType? GameType { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}