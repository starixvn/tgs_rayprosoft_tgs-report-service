using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel.BrandGameProvider;

namespace Tgs.Report.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandGameProviderReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BrandGameProviderReportController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [Description("Get brand game provider round aggregate")]
        public Task<GetBrandGameProviderRoundsResponse> GetBrandGameProviderRoundsAsync(
            [FromQuery] GetBrandGameProviderRoundsRequest request) =>
            _mediator.Send(request);
    }
}