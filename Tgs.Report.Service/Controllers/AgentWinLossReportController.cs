using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AgentWinLossReportController : ControllerBase
{
    private readonly IMediator _mediator;

    public AgentWinLossReportController(IMediator mediator) => _mediator = mediator;

    [HttpGet("agentWinLoss")]
    [Description("Get agent Win/Loss report")]
    public Task<PagedResponse<AgentWinLossModel>> GetAgentWinLossAsync(
    [FromQuery] GetAgentWinLossRequest request) =>
        _mediator.Send(request);

    [HttpGet("downLineAgentWinLoss")]
    [Description("Get down line agent Win/Loss report")]
    public Task<PagedResponse<AgentWinLossModel>> GetDownLineAgentWinLossAsync(
        [FromQuery] GetDownLineAgentWinLossRequest request) =>
        _mediator.Send(request);

    [HttpGet("masterAgentWinLoss")]
    [Description("Get master agent Win/Loss report")]
    public Task<PagedResponse<AgentWinLossModel>> GetMasterAgentWinLossAsync(
        [FromQuery] GetMasterAgentWinLossRequest request) =>
        _mediator.Send(request);

    [HttpGet("agentPlayerWinLoss")]
    [Description("Get agent player Win/Loss report")]
    public Task<PagedResponse<AgentWinLossModel>> GetAgentPlayerWinLossAsync(
        [FromQuery] GetPlayerWinLossRequest request) =>
        _mediator.Send(request);

    [HttpGet("breadcrumb")]
    [Description("Get Breadcrumb")]
    public Task<IEnumerable<BreadcrumbModel>> GetBreadcrumbAsync(
        [FromQuery] GetBreadcrumbRequest request) =>
        _mediator.Send(request);

    [HttpGet("agentPlayerSpecialDetail")]
    [Description("Get Agent Player Special Detail")]
    public Task<AgentPlayerSpecialDetailModel> GetAgentPlayerSpecialDetailAsync(
        [FromQuery] GetAgentPlayerSpecialDetailRequest request) =>
        _mediator.Send(request);

    [HttpGet("agentTotalDetail")]
    [Description("Get Agent Total Detail")]
    public Task<IEnumerable<AgentTotalDetailModel>> GetAgentTotalDetailAsync(
        [FromQuery] GetAgentTotalDetailRequest request) =>
        _mediator.Send(request);

    [HttpGet("companyTotalDetail")]
    [Description("Get Company Total Detail")]
    public Task<IEnumerable<CompanyTotalDetailModel>> GetCompanyTotalDetailAsync(
        [FromQuery] GetCompanyTotalDetailRequest request) =>
        _mediator.Send(request);
}