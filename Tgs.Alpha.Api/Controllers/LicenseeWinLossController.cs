using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel.LicenseeWinLoss;

namespace Tgs.Alpha.Api.Controllers;

[Route("api/[controller]")]
[Description("Get licensee win loss by reference id")]
[ApiController]
public class LicenseeWinLossController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public Task<IEnumerable<LicenseeWinLossModel>> GetWinLossReportByReferenceAsync(
        [FromQuery] GetLicenseeWinLossByRefIdRequest request) =>
        mediator.Send(request);
}
