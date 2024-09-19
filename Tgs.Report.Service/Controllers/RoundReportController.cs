using System.ComponentModel;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoundReportController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetRounds")]
    [Description("Get rounds")]
    public Task<PagedResponse<RoundModel>> GetRoundsAsync([FromQuery] GetRoundsRequest request) =>
        mediator.Send(request);

    [HttpGet("system", Name = "GetSystemRounds")]
    [Description("Get rounds")]
    public Task<PagedResponse<SystemRoundModel>> GetSystemRoundsAsync([FromQuery] GetSystemRoundsRequest request) =>
        mediator.Send(request);

    [HttpGet("exportSystemRounds")]
    [Description("export rounds")]
    public async Task<IActionResult> ExportSystemRoundsAsync([FromQuery] ExportSystemRoundsRequest request) => File(
        await mediator.Send(request).ConfigureAwait(false),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

    [HttpGet("roundsTotal", Name = "GetRoundsTotal")]
    [Description("Get rounds Total BetAmount & ValidBetAmount & WinLoss")]
    public Task<RoundTotalModel> GetRoundsTotalAsync([FromQuery] GetRoundsTotalRequest request) =>
        mediator.Send(request);

    [HttpGet("systemRoundsTotal", Name = "GetSystemRoundsTotal")]
    [Description("Get rounds Total BetAmount & ValidBetAmount & Turnover & SettleAmount & WinLoss")]
    public Task<SystemRoundTotalModel> GetSystemRoundsTotalAsync([FromQuery] GetSystemRoundsTotalRequest request) =>
        mediator.Send(request);

    [HttpGet("betRecords", Name = "GetBetRecords")]
    [Description("Get bet records")]
    public Task<GetBetRecordsResponse> GetBetRecordsAsync([FromQuery] GetBetRecordsRequest request) =>
        mediator.Send(request);

    [HttpGet("agentPlayerRounds", Name = "GetAgentPlayerRounds")]
    [Description("Get agent player rounds")]
    public Task<PagedResponse<RoundModel>> GetAgentPlayerRoundsAsync(
        [FromQuery] GetAgentPlayerRoundsRequest request) =>
        mediator.Send(request);

    [HttpGet("{roundId}", Name = "GetRound")]
    [Description("Get a round")]
    public Task<JsonDocument?> GetRoundAsync(Guid roundId, [FromQuery] GetRoundHistoryRequest request)
    {
        request.RoundId = roundId;
        return mediator.Send(request);
    }

    [HttpGet("closedRounds", Name = "GetClosedRounds")]
    [Description("Get closed rounds")]
    public async Task<PagedResponse<ClosedRoundModel>> GetClosedRoundsAsync([FromQuery] GetClosedRoundsRequest request) 
    {
        var res = await mediator.Send(request);

        return res;
    }

    [HttpGet("openRounds", Name = "GetOpenRounds")]
    [Description("Get open rounds")]
    public async Task<PagedResponse<OpenRoundModel>> GetOpenRoundsAsync([FromQuery] GetOpenRoundsRequest request)
    {
        var res = await mediator.Send(request);

        return res;
    }

    [HttpGet("exportRounds")]
    [Description("export rounds")]
    public async Task<IActionResult> ExportRoundsAsync([FromQuery] ExportRoundsRequest request) => File(
        await mediator.Send(request).ConfigureAwait(false),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
}