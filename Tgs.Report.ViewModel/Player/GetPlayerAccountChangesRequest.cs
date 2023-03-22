using System;
using MediatR;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Player;

public record GetPlayerAccountChangesRequest : PageableRequest, IRequest<PagedResponse<PlayerAccountChangeModel>>
{
    public Guid BrandId { get; init; }
    public string PlayerCode { get; init; } = default!;
    public AccountChangeType? AccountChangeType { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}