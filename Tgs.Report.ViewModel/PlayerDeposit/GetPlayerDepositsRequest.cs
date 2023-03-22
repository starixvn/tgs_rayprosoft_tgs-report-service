using System;
using MediatR;

namespace Tgs.Report.ViewModel.PlayerDeposit;

public record GetPlayerDepositsRequest : PageableRequest, IRequest<PagedResponse<PlayerDepositModel>>
{
    public Guid BrandId { get; init; }
    public string AgentUsername { get; init; } = default!;
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}