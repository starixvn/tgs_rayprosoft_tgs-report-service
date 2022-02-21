using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Vip;

namespace Tgs.Report.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VipReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VipReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Description("Get vips")]
        public Task<PagedResponse<VipModel>> GetVipsAsync([FromQuery] GetVipReportRequest request) =>
            _mediator.Send(request);

        [HttpGet("{username}")]
        [Description("Get vip rebates")]
        public Task<PagedResponse<VipRebateModel>> GetVipRebatesAsync(string username,
            [FromQuery] GetVipRebateReportRequest request)
        {
            request.Username = username;
            return _mediator.Send(request);
        }

        [HttpGet("exportVips")]
        [Description("export vips")]
        public async Task<IActionResult> ExportVipsAsync(
            [FromQuery] ExportVipReportRequest request) => File(
            await _mediator.Send(request).ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}