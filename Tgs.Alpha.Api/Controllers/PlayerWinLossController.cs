using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel.LicenseeWinLoss;

namespace Tgs.Alpha.Api.Controllers;

[Route("api/[controller]")]
[Description("Get licensee win loss by reference id")]
[ApiController]
public class PlayerWinLossController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public Task<IEnumerable<PlayerWinLossModel>> GetWinLossReportByReferenceAsync(
        [FromQuery] GetPlayerWinLossByRefIdRequest request) =>
        mediator.Send(request);
}
