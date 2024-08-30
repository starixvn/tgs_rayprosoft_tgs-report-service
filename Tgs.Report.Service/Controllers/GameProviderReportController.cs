using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Infrastructures.Extensions.Models;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.GameProvider;

namespace Tgs.Report.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameProviderReportController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Description("Get game provider round aggregate")]
    public Task<PagedResponse<GameProviderRoundsModel>> GetGameProviderRoundsAsync(
        [FromQuery] GetGameProviderRoundsRequest request) =>
        mediator.Send(request);
}