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
    public Task<IEnumerable<AgentPlayerSpecialDetailModel>> GetAgentPlayerSpecialDetailAsync(
        [FromQuery] GetAgentPlayerSpecialDetailRequest request) =>
        _mediator.Send(request);

    [HttpGet("agentSpecialDetail")]
    [Description("Get Agent Special Detail")]
    public Task<IEnumerable<AgentSpecialDetailModel>> GetAgentSpecialDetailAsync(
        [FromQuery] GetAgentSpecialDetailRequest request) =>
        _mediator.Send(request);

    [HttpGet("companySpecialDetail")]
    [Description("Get Company Special Detail")]
    public Task<IEnumerable<CompanySpecialDetailModel>> GetCompanySpecialDetailAsync(
        [FromQuery] GetCompanySpecialDetailRequest request) =>
        _mediator.Send(request);
}