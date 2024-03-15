using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.GameAction;

namespace Tgs.Report.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameActionReportController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Description("Get game actions")]
    public Task<PagedResponse<GameActionModel>> GetGameActionsAsync(
        [FromQuery] GetGameActionsRequest request) =>
        mediator.Send(request);
}