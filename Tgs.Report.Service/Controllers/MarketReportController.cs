using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Market;

namespace Tgs.Report.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MarketReportController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Description("Get markets")]
    public Task<PagedResponse<MarketModel>> GetMarketsAsync([FromQuery] GetMarketsRequest request) =>
        mediator.Send(request);

    [HttpGet("agentProfit")]
    [Description("Get markets")]
    public Task<PagedResponse<MarketAgentProfitModel>> GetMarketAgentProfitAsync(
        [FromQuery] GetMarketAgentProfitsRequest request) =>
        mediator.Send(request);

    [HttpGet("additionalExpenses")]
    [Description("Get Additional Expenses")]
    public Task<GetAdditionalExpensesResponse> GetAdditionalExpensesAsync(
        [FromQuery] GetAdditionalExpensesRequest request) =>
        mediator.Send(request);

    [HttpGet("exportMarkets")]
    [Description("export markets")]
    public async Task<IActionResult> ExportMarketsAsync([FromQuery] ExportMarketsRequest request) => File(
        await mediator.Send(request).ConfigureAwait(false),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
}