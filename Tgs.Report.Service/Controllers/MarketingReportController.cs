using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Market;
using Tgs.Report.ViewModel.Marketing;

namespace Tgs.Report.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MarketingReportController : ControllerBase
{
    private readonly IMediator _mediator;

    public MarketingReportController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Description("Get marketing list")]
    public Task<PagedResponse<MarketingModel>> GetMarketingListAsync([FromQuery] GetMarketingListRequest request) =>
        _mediator.Send(request);

    [HttpGet("registerDetail")]
    [Description("Get register details")]
    public Task<PagedResponse<MarketingRegisterDetailModel>> GetMarketingRegisterDetailsAsync(
        [FromQuery] GetMarketingRegisterDetailRequest request) =>
        _mediator.Send(request);

    [HttpGet("firstDepositDetail")]
    [Description("Get first deposit details")]
    public Task<PagedResponse<MarketingFirstDepositDetailModel>> GetMarketingFirstDepositDetailsAsync(
        [FromQuery] GetMarketingFirstDepositDetailRequest request) =>
        _mediator.Send(request);

    [HttpGet("gamingDetail")]
    [Description("Get gaming details")]
    public Task<PagedResponse<MarketingGamingDetailModel>> GetMarketingGamingDetailsAsync(
        [FromQuery] GetMarketingGamingDetailRequest request) =>
        _mediator.Send(request);
}