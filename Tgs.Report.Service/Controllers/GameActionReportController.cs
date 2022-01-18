using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.GameAction;

namespace Tgs.Report.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameActionReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GameActionReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Description("Get game actions")]
        public Task<PagedResponse<GameActionModel>> GetGameActionsAsync(
            [FromQuery] GetGameActionsRequest request) =>
            _mediator.Send(request);
    }
}