using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel.LicenseeMarket;

namespace Tgs.Report.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LicenseeMarketReportController(IMediator mediator) : ControllerBase
{
    [HttpGet("ExportLicenseeMarkets")]
    [Description("Export licensee markets")]
    public async Task<IActionResult> ExportLicenseeMarketsAsync([FromQuery] ExportLicenseeMarketsRequest request)
        => File(
            await mediator.Send(request).ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

    [HttpGet]
    [Description("Get licensee markets")]
    public Task<LicenseeMarketsResponse> GetLicenseeMarketsAsync([FromQuery] GetLicenseeMarketsRequest request) =>
        mediator.Send(request);
}