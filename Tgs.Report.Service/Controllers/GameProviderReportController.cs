using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.GameProvider;

namespace Tgs.Report.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameProviderReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GameProviderReportController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [Description("Get game provider round aggregate")]
        public Task<PagedResponse<GameProviderRoundsModel>> GetGameProviderRoundsAsync(
            [FromQuery] GetGameProviderRoundsRequest request) =>
            _mediator.Send(request);
    }
}