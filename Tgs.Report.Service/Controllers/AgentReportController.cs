using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;
using Tgs.Report.ViewModel.AgentWinLoss;
using TGS.Core.Infrastructures.Extensions;

namespace Tgs.Report.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AgentReportController(IMediator mediator) : ControllerBase
{
    [HttpGet("contribution")]
    [Description("Get agent contributions")]
    public Task<PagedResponse<AgentContributionModel>> GetAgentContributionsAsync(
        [FromQuery] GetAgentContributionsRequest request) =>
        mediator.Send(request);

    [HttpGet("dividends")]
    [Description("Get agent dividends report")]
    public Task<PagedResponse<AgentDividendModel>>
        GetAgentDividendsAsync([FromQuery] GetAgentDividendsRequest request) =>
        mediator.Send(request);

    [HttpGet("singleAgentDividends")]
    [Description("Get single agent dividends report")]
    public Task<PagedResponse<AgentDividendModel>>
        GetAgentDividendsAsync([FromQuery] GetSingleAgentDividendsRequest request) =>
        mediator.Send(request);

    [HttpGet("subAgentDividends")]
    [Description("Get sub agent dividends report")]
    public Task<PagedResponse<AgentDividendModel>>
        GetAgentDividendsAsync([FromQuery] GetSubAgentDividendsRequest request) =>
        mediator.Send(request);

    [HttpGet("dividendDetails")]
    [Description("Get agent dividend details report")]
    public Task<PagedResponse<AgentDividendDetailModel>>
        GetAgentDividendDetailsAsync([FromQuery] GetAgentDividendDetailsRequest request) =>
        mediator.Send(request);

    [HttpGet("dividendTeamDetails")]
    [Description("Get agent dividend team details report")]
    public Task<PagedResponse<AgentDividendTeamDetailModel>>
        GetAgentDividendDetailsAsync([FromQuery] GetAgentDividendTeamDetailsRequest request) =>
        mediator.Send(request);

    [HttpGet("{id}/winLoss")]
    [Description("Get agent player Win/Loss report")]
    public Task<PagedResponse<AgentPlayerWinLossModel>> GetAgentPlayerWinLossAsync(
        [FromQuery] GetAgentDividendPlayerWinLossRequest request) =>
        mediator.Send(request);

    [HttpGet("accountChanges")]
    [Description("Get agent account changes")]
    public Task<PagedResponse<AgentAccountChangeModel>> GetAgentAccountChangesAsync(
        [FromQuery] GetAgentAccountChangesRequest request) =>
        mediator.Send(request);

    [HttpGet("exportAgentContributions")]
    [Description("export agent contributions")]
    public async Task<IActionResult> ExportAgentContributionsAsync(
        [FromQuery] ExportAgentContributionsRequest request)
        => File(
            await mediator.Send(request).ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

    [HttpGet("exportAgentDividends")]
    [Description("export agent dividends")]
    public async Task<IActionResult> ExportAgentDividendsAsync([FromQuery] ExportAgentDividendsRequest request)
        => File(
            await mediator.Send(request).ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

    [HttpGet("exportAgentDividendsTrialCalculation")]
    [Description("export agent dividends trial calculation")]
    public async Task<IActionResult> ExportAgentDividendsTrialCalculationAsync(
        [FromQuery] ExportAgentDividendsTrialCalculationRequest request)
        => File(
            await mediator.Send(request).ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
}