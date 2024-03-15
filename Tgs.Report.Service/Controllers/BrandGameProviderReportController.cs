using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel.BrandGameProvider;

namespace Tgs.Report.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BrandGameProviderReportController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Description("Get brand game provider round aggregate")]
    public Task<GetBrandGameProviderRoundsResponse> GetBrandGameProviderRoundsAsync(
        [FromQuery] GetBrandGameProviderRoundsRequest request) =>
        mediator.Send(request);

    [HttpGet("V2")]
    [Description("Get brand game provider aggregate")]
    public Task<GetBrandGameProvidersResponse> GetBrandGameProvidersAsync(
        [FromQuery] GetBrandGameProvidersRequest request) =>
        mediator.Send(request);
}