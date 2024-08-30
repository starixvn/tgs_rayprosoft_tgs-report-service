using System;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.ViewModel.GameAction;

public record GetGameActionsRequest : PageableRequest, IRequest<PagedResponse<GameActionModel>>
{
    public Guid RoundId { get; init; }
}