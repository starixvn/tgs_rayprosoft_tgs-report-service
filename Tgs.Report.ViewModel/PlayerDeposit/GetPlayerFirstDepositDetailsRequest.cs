using System;
using MediatR;

namespace Tgs.Report.ViewModel.PlayerDeposit
{
    public record GetPlayerFirstDepositDetailsRequest : PageableRequest, IRequest<PagedResponse<PlayerFirstDepositDetailModel>>
    {
        public Guid BrandId { get; init; }
        public string AgentUsername { get; init; } = default!;
        public DateTimeOffset Date { get; init; }
    }
}