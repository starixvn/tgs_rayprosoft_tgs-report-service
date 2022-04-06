using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel.ResellerMarket;

namespace Tgs.Report.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResellerMarketReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ResellerMarketReportController(IMediator mediator) => _mediator = mediator;

        [HttpGet("ExportResellerMarkets")]
        [Description("Export reseller markets")]
        public async Task<IActionResult> ExportResellerMarketsAsync([FromQuery] ExportResellerMarketsRequest request)
            => File(
                await _mediator.Send(request).ConfigureAwait(false),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        [HttpGet]
        [Description("Get reseller markets")]
        public Task<ResellerMarketsResponse> GetResellerMarketsAsync([FromQuery] GetResellerMarketsRequest request) =>
            _mediator.Send(request);
    }
}