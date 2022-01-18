using System;
using MediatR;

namespace Tgs.Report.ViewModel.GameAction
{
    public class GetGameActionsRequest : PageableRequest, IRequest<PagedResponse<GameActionModel>>
    {
        public Guid RoundId { get; set; }
    }
}