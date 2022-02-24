using System.ComponentModel;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoundReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RoundReportController(IMediator mediator) => _mediator = mediator;

        [HttpGet(Name = "GetRounds")]
        [Description("Get rounds")]
        public Task<PagedResponse<RoundModel>> GetRoundsAsync([FromQuery] GetRoundsRequest request) =>
            _mediator.Send(request);

        [HttpGet("agentPlayerRounds", Name = "GetAgentPlayerRounds")]
        [Description("Get agent player rounds")]
        public Task<PagedResponse<RoundModel>> GetAgentPlayerRoundsAsync(
            [FromQuery] GetAgentPlayerRoundsRequest request) =>
            _mediator.Send(request);

        [HttpGet("{roundId}", Name = "GetRound")]
        [Description("Get a round")]
        public Task<JsonDocument?> GetRoundAsync(Guid roundId, [FromQuery] GetRoundHistoryRequest request)
        {
            request.RoundId = roundId;
            return _mediator.Send(request);
        }

        [HttpGet("closedRounds", Name = "GetClosedRounds")]
        [Description("Get closed rounds")]
        public Task<PagedResponse<ClosedRoundModel>> GetClosedRoundsAsync([FromQuery] GetClosedRoundsRequest request) =>
            _mediator.Send(request);

        [HttpGet("openRounds", Name = "GetOpenRounds")]
        [Description("Get open rounds")]
        public Task<PagedResponse<OpenRoundModel>> GetOpenRoundsAsync([FromQuery] GetOpenRoundsRequest request) =>
            _mediator.Send(request);
    }
}