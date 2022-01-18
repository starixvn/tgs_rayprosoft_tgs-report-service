using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Market;

namespace Tgs.Report.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MarketReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Description("Get markets")]
        public Task<PagedResponse<MarketModel>> GetMarketsAsync([FromQuery] GetMarketsRequest request) =>
            _mediator.Send(request);

        [HttpGet("additionalExpenses")]
        [Description("Get Additional Expenses")]
        public Task<GetAdditionalExpensesResponse> GetAdditionalExpensesAsync(
            [FromQuery] GetAdditionalExpensesRequest request) =>
            _mediator.Send(request);

        [HttpGet("exportMarkets")]
        [Description("export markets")]
        public async Task<IActionResult> ExportMarketsAsync([FromQuery] ExportMarketsRequest request) => File(
            await _mediator.Send(request).ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}