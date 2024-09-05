using System;
using System.Collections.Generic;
using MediatR;

namespace Tgs.Report.ViewModel.LicenseeWinLoss;

public record GetLicenseeWinLossByRefIdRequest : IRequest<IEnumerable<LicenseeWinLossModel>>
{
    public string? ReferenceId { get; init; }

    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }
}
