using System;
using MediatR;

namespace Tgs.Report.ViewModel.GameAction;

public record GetGameActionsRequest : PageableRequest, IRequest<PagedResponse<GameActionModel>>
{
    public Guid RoundId { get; init; }
}