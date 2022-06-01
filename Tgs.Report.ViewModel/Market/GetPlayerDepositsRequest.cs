using System;
using MediatR;

namespace Tgs.Report.ViewModel.Market
{
    public record GetPlayerDepositsRequest : PageableRequest, IRequest<PagedResponse<PlayerDepositModel>>
    {
        public Guid BrandId { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}