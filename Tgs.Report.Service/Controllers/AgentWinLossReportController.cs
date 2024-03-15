using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AgentWinLossReportController(IMediator mediator) : ControllerBase
{
    [HttpGet("agentWinLoss")]
    [Description("Get agent Win/Loss report")]
    public Task<PagedResponse<AgentWinLossModel>> GetAgentWinLossAsync(
    [FromQuery] GetAgentWinLossRequest request) =>
        mediator.Send(request);

    [HttpGet("downLineAgentWinLoss")]
    [Description("Get down line agent Win/Loss report")]
    public Task<PagedResponse<AgentWinLossModel>> GetDownLineAgentWinLossAsync(
        [FromQuery] GetDownLineAgentWinLossRequest request) =>
        mediator.Send(request);

    [HttpGet("masterAgentWinLoss")]
    [Description("Get master agent Win/Loss report")]
    public Task<PagedResponse<AgentWinLossModel>> GetMasterAgentWinLossAsync(
        [FromQuery] GetMasterAgentWinLossRequest request) =>
        mediator.Send(request);

    [HttpGet("agentPlayerWinLoss")]
    [Description("Get agent player Win/Loss report")]
    public Task<PagedResponse<AgentWinLossModel>> GetAgentPlayerWinLossAsync(
        [FromQuery] GetPlayerWinLossRequest request) =>
        mediator.Send(request);

    [HttpGet("breadcrumb")]
    [Description("Get Breadcrumb")]
    public Task<IEnumerable<BreadcrumbModel>> GetBreadcrumbAsync(
        [FromQuery] GetBreadcrumbRequest request) =>
        mediator.Send(request);

    [HttpGet("agentPlayerSpecialDetail")]
    [Description("Get Agent Player Special Detail")]
    public Task<AgentPlayerSpecialDetailModel> GetAgentPlayerSpecialDetailAsync(
        [FromQuery] GetAgentPlayerSpecialDetailRequest request) =>
        mediator.Send(request);

    [HttpGet("agentTotalDetail")]
    [Description("Get Agent Total Detail")]
    public Task<IEnumerable<AgentTotalDetailModel>> GetAgentTotalDetailAsync(
        [FromQuery] GetAgentTotalDetailRequest request) =>
        mediator.Send(request);

    [HttpGet("companyTotalDetail")]
    [Description("Get Company Total Detail")]
    public Task<IEnumerable<CompanyTotalDetailModel>> GetCompanyTotalDetailAsync(
        [FromQuery] GetCompanyTotalDetailRequest request) =>
        mediator.Send(request);

    [HttpGet("exportAgentWinLoss")]
    [Description("export agent WinLoss")]
    public async Task<IActionResult> ExportAgentWinLossAsync([FromQuery] ExportAgentWinLossRequest request)
        => File(
            await mediator.Send(request).ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

    [HttpGet("exportDownLineAgentWinLoss")]
    [Description("export downLine agent WinLoss")]
    public async Task<IActionResult> ExportDownLineAgentWinLossAsync([FromQuery] ExportDownLineAgentWinLossRequest request)
        => File(
            await mediator.Send(request).ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

    [HttpGet("exportMasterAgentWinLoss")]
    [Description("export master agent WinLoss")]
    public async Task<IActionResult> ExportMasterAgentWinLossAsync([FromQuery] ExportMasterAgentWinLossRequest request)
        => File(
            await mediator.Send(request).ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

    [HttpGet("exportPlayerWinLoss")]
    [Description("export player WinLoss")]
    public async Task<IActionResult> ExportPlayerWinLossAsync([FromQuery] ExportPlayerWinLossRequest request)
        => File(
            await mediator.Send(request).ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
}