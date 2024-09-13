using System;
using System.Collections.Generic;
using MediatR;

namespace Tgs.Report.ViewModel.LicenseeWinLoss;

public record GetPlayerWinLossByRefIdRequest : IRequest<IEnumerable<PlayerWinLossModel>>
{
    public string? ReferenceIds { get; init; }

    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }
}
