using System;
using MediatR;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Player;

public record GetPlayersAccountChangesTotalAmountRequest : IRequest<PlayersAccountChangesTotalAmountModel>
{
    public Guid BrandId { get; init; }
    public string? Username { get; init; }
    public TransactionType? TransactionType { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}