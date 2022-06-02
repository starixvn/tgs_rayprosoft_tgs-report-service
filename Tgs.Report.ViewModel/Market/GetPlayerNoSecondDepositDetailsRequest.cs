using System;
using MediatR;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.ViewModel.Market
{
    public record GetPlayerNoSecondDepositDetailsRequest : PageableRequest, IRequest<PagedResponse<PlayerNoSecondDepositDetailModel>>
    {
        public Guid BrandId { get; init; }
        public DateTimeOffset Date { get; init; }
    }
}