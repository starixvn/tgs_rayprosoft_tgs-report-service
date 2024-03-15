using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlayerReportController(IMediator mediator) : ControllerBase
{
    [HttpGet("contribution", Name = "GetPlayerContributions")]
    [Description("Get player contributions")]
    public Task<PagedResponse<PlayerContributionModel>> GetPlayerContributionsAsync(
        [FromQuery] GetPlayerContributionsRequest request)
        => mediator.Send(request);

    [HttpGet("contributionTotalAmount", Name = "GetPlayerContributionsTotalAmount")]
    [Description("Get player contribution total amount")]
    public Task<PlayerContributionTotalAmountModel> GetPlayerContributionsTotalAmountAsync(
        [FromQuery] GetPlayerContributionsTotalAmountRequest request)
        => mediator.Send(request);

    [HttpGet("accountChanges", Name = "GetPlayerAccountChanges")]
    [Description("Get player account changes")]
    public Task<PagedResponse<PlayerAccountChangeModel>> GetPlayerAccountChangesAsync(
        [FromQuery] GetPlayerAccountChangesRequest request)
        => mediator.Send(request);

    [HttpGet("accountChangesTotal", Name = "GetPlayerAccountChangesTotalAmount")]
    [Description("Get player account changes total amount")]
    public Task<decimal> GetPlayerAccountChangesTotalAmountAsync(
        [FromQuery] GetPlayerAccountChangesTotalAmountRequest request)
        => mediator.Send(request);

    [HttpGet("exportPlayerContributions", Name = "ExportPlayerContributions")]
    [Description("export player contributions")]
    public async Task<IActionResult> ExportPlayerContributionsAsync(
        [FromQuery] ExportPlayerContributionsRequest request)
        => File(await mediator.Send(request).ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

    [HttpGet("playersAccountChanges", Name = "GetPlayersAccountChanges")]
    [Description("Get account changes of multiple players")]
    public Task<PagedResponse<PlayersAccountChangeModel>> GetPlayersAccountChangesAsync(
        [FromQuery] GetPlayersAccountChangesRequest request)
        => mediator.Send(request);

    [HttpGet("player-all-agent-rebates", Name = "GetPlayerAllAgentRebates")]
    [Description("Get player all agent rebates")]
    public IAsyncEnumerable<PlayerAllAgentRebateModel> GetPlayerAllAgentRebatesAsync(
        [FromQuery] GetPlayerAllAgentRebatesRequest request)
        => mediator.CreateStream(request);

    [HttpGet("player-all-agent-rebates-total-amount", Name = "GetPlayerAllAgentRebatesTotalAmount")]
    [Description("Get player all agent rebates total amount")]
    public Task<PlayerAllAgentRebateTotalAmountModel> GetPlayerAllAgentRebatesTotalAmountAsync(
        [FromQuery] GetPlayerAllAgentRebateTotalAmountRequest request)
        => mediator.Send(request);

    [HttpGet("player-all-agent-rebate-detail-game-types", Name = "GetPlayerAllAgentRebateDetailGameTypes")]
    [Description("Get player all agent rebate detail game types")]
    public IAsyncEnumerable<PlayerAllAgentRebateDetailGameTypeModel> GetPlayerAllAgentRebateDetailGameTypesAsync(
        [FromQuery] GetPlayerAllAgentRebateDetailGameTypesRequest request)
        => mediator.CreateStream(request);

    [HttpGet("player-all-agent-rebate-details", Name = "GetPlayerAllAgentRebateDetails")]
    [Description("Get player all agent rebate details")]
    public IAsyncEnumerable<PlayerAllAgentRebateDetailModel> GetPlayerAllAgentRebateDetailsAsync(
        [FromQuery] GetPlayerAllAgentRebateDetailsRequest request)
        => mediator.CreateStream(request);
}