using System;
using MediatR;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.ViewModel.Market
{
    public record GetPlayerFirstDepositDetailsRequest : PageableRequest, IRequest<PagedResponse<PlayerFirstDepositDetailModel>>
    {
        public Guid BrandId { get; init; }
        public DateTimeOffset Date { get; init; }
    }
}