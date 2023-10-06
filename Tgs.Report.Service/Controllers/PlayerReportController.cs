using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlayerReportController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlayerReportController(IMediator mediator) => _mediator = mediator;

    [HttpGet("contribution", Name = "GetPlayerContributions")]
    [Description("Get player contributions")]
    public Task<PagedResponse<PlayerContributionModel>> GetPlayerContributionsAsync(
        [FromQuery] GetPlayerContributionsRequest request)
        => _mediator.Send(request);

    [HttpGet("contributionTotalAmount", Name = "GetPlayerContributionsTotalAmount")]
    [Description("Get player contribution total amount")]
    public Task<PlayerContributionTotalAmountModel> GetPlayerContributionsTotalAmountAsync(
        [FromQuery] GetPlayerContributionsTotalAmountRequest request)
        => _mediator.Send(request);

    [HttpGet("accountChanges", Name = "GetPlayerAccountChanges")]
    [Description("Get player account changes")]
    public Task<PagedResponse<PlayerAccountChangeModel>> GetPlayerAccountChangesAsync(
        [FromQuery] GetPlayerAccountChangesRequest request)
        => _mediator.Send(request);

    [HttpGet("accountChangesTotal", Name = "GetPlayerAccountChangesTotalAmount")]
    [Description("Get player account changes total amount")]
    public Task<decimal> GetPlayerAccountChangesTotalAmountAsync(
        [FromQuery] GetPlayerAccountChangesTotalAmountRequest request)
        => _mediator.Send(request);

    [HttpGet("exportPlayerContributions", Name = "ExportPlayerContributions")]
    [Description("export player contributions")]
    public async Task<IActionResult> ExportPlayerContributionsAsync(
        [FromQuery] ExportPlayerContributionsRequest request)
        => File(await _mediator.Send(request).ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

    [HttpGet("playersAccountChanges", Name = "GetPlayersAccountChanges")]
    [Description("Get account changes of multiple players")]
    public Task<PagedResponse<PlayersAccountChangeModel>> GetPlayersAccountChangesAsync(
        [FromQuery] GetPlayersAccountChangesRequest request)
        => _mediator.Send(request);

    [HttpGet("player-all-agent-rebates", Name = "GetPlayerAllAgentRebates")]
    [Description("Get player all agent rebates")]
    public IAsyncEnumerable<PlayerAllAgentRebateModel> GetPlayerAllAgentRebatesAsync(
        [FromQuery] GetPlayerAllAgentRebatesRequest request)
        => _mediator.CreateStream(request);

    [HttpGet("player-all-agent-rebates-total-amount", Name = "GetPlayerAllAgentRebatesTotalAmount")]
    [Description("Get player all agent rebates total amount")]
    public Task<PlayerAllAgentRebateTotalAmountModel> GetPlayerAllAgentRebatesTotalAmountAsync(
        [FromQuery] GetPlayerAllAgentRebateTotalAmountRequest request)
        => _mediator.Send(request);

    [HttpGet("player-all-agent-game-types", Name = "GetPlayerAllAgentGameTypes")]
    [Description("Get player all agent game types")]
    public IAsyncEnumerable<PlayerAllAgentGameTypeModel> GetPlayerAllAgentGameTypesAsync(
        [FromQuery] GetPlayerAllAgentGameTypesRequest request)
        => _mediator.CreateStream(request);

    [HttpGet("player-all-agent-rebate-details", Name = "GetPlayerAllAgentRebateDetails")]
    [Description("Get player all agent rebate details")]
    public IAsyncEnumerable<PlayerAllAgentRebateDetailModel> GetPlayerAllAgentRebateDetailsAsync(
        [FromQuery] GetPlayerAllAgentRebatesDetailsRequest request)
        => _mediator.CreateStream(request);
}