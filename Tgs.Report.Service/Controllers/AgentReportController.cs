using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AgentReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("contribution")]
        [Description("Get agent contributions")]
        public Task<PagedResponse<AgentContributionModel>> GetAgentContributionsAsync(
            [FromQuery] GetAgentContributionsRequest request) =>
            _mediator.Send(request);


        [HttpGet("{id}/teamInfo")]
        [Description("Get agent team Information")]
        public Task<PagedResponse<TeamInfoModel>> GetAgentTeamInformationAsync(
            Guid id,
            [FromQuery] GetAgentTeamInfoRequest request)
        {
            request.AgentId = id;

            return _mediator.Send(request);
        }

        [HttpGet("dividends")]
        [Description("Get agent dividends report")]
        public Task<PagedResponse<AgentDividendModel>>
            GetAgentDividendsAsync([FromQuery] GetAgentDividendsRequest request) =>
            _mediator.Send(request);

        [HttpGet("dividendDetails")]
        [Description("Get agent dividend details report")]
        public Task<PagedResponse<AgentDividendDetailModel>>
            GetAgentDividendDetailsAsync([FromQuery] GetAgentDividendDetailsRequest request) =>
            _mediator.Send(request);

        [HttpGet("dividendTeamDetails")]
        [Description("Get agent dividend team details report")]
        public Task<PagedResponse<AgentDividendTeamDetailModel>>
            GetAgentDividendDetailsAsync([FromQuery] GetAgentDividendTeamDetailsRequest request) =>
            _mediator.Send(request);

        [HttpGet("{id}/winLoss")]
        [Description("Get agent player Win/Loss report")]
        public Task<PagedResponse<AgentPlayerWinLossModel>> GetAgentPlayerWinLossAsync(
            [FromQuery] GetAgentPlayerWinLossRequest request) =>
            _mediator.Send(request);

        [HttpGet("accountChanges")]
        [Description("Get agent account changes")]
        public Task<PagedResponse<AgentAccountChangeModel>> GetAgentAccountChangesAsync(
            [FromQuery] GetAgentAccountChangesRequest request) =>
            _mediator.Send(request);

        [HttpGet("exportAgentContributions")]
        [Description("export agent contributions")]
        public async Task<IActionResult> ExportAgentContributionsAsync(
            [FromQuery] ExportAgentContributionsRequest request)
            => File(
                await _mediator.Send(request).ConfigureAwait(false),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        [HttpGet("exportAgentDividends")]
        [Description("export agent dividends")]
        public async Task<IActionResult> ExportAgentDividendsAsync([FromQuery] ExportAgentDividendsRequest request)
            => File(
                await _mediator.Send(request).ConfigureAwait(false),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        [HttpGet("exportAgentDividendsTrialCalculation")]
        [Description("export agent dividends trial calculation")]
        public async Task<IActionResult> ExportAgentDividendsTrialCalculationAsync(
            [FromQuery] ExportAgentDividendsTrialCalculationRequest request)
            => File(
                await _mediator.Send(request).ConfigureAwait(false),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}