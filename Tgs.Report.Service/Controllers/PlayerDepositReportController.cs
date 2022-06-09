using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.PlayerDeposit;

namespace Tgs.Report.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlayerDepositReportController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlayerDepositReportController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Description("Get player deposits")]
    public Task<PagedResponse<PlayerDepositModel>> GetPlayerDepositsAsync(
        [FromQuery] GetPlayerDepositsRequest request) =>
        _mediator.Send(request);

    [HttpGet("playerFirstDepositDetails")]
    [Description("Get player first deposit details")]
    public Task<PagedResponse<PlayerFirstDepositDetailModel>> GetPlayerFirstDepositDetailsAsync(
        [FromQuery] GetPlayerFirstDepositDetailsRequest request) =>
        _mediator.Send(request);

    [HttpGet("playerNoSecondDepositDetails")]
    [Description("Get player no second deposit details")]
    public Task<PagedResponse<PlayerNoSecondDepositDetailModel>> GetPlayerNoSecondDepositDetailsAsync(
        [FromQuery] GetPlayerNoSecondDepositDetailsRequest request) =>
        _mediator.Send(request);
}