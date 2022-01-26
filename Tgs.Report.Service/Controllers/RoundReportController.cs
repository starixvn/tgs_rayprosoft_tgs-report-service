using System.ComponentModel;
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

        public RoundReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Description("Get rounds")]
        public Task<PagedResponse<RoundModel>> GetRoundsAsync([FromQuery] GetRoundsRequest request) =>
            _mediator.Send(request);

        [HttpGet("{roundId}")]
        [Description("Get a round")]
        public Task<GetRoundHistoryResponse> GetRoundAsync(Guid roundId, [FromQuery] GetRoundHistoryRequest request)
        {
            request.RoundId = roundId;
            return _mediator.Send(request);
        }

        [HttpGet("openRounds")]
        [Description("Get open rounds")]
        public Task<PagedResponse<OpenRoundModel>> GetOpenRoundsAsync([FromQuery] GetOpenRoundsRequest request) =>
            _mediator.Send(request);
    }
}