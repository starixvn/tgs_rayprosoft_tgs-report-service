using System;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.ViewModel.PlayerDeposit;

public record GetPlayerNoSecondDepositDetailsRequest : PageableRequest,
    IRequest<PagedResponse<PlayerNoSecondDepositDetailModel>>
{
    public Guid BrandId { get; init; }
    public string AgentUsername { get; init; } = default!;
    public DateTimeOffset Date { get; init; }
}