using System;

namespace Tgs.Report.ViewModel.LicenseeWinLoss;

public record PlayerWinLossModel
{
    public string? GameProviderCode { get; init; }

    public string? WinLossDate { get; set; }

    public decimal PlayerWinLoss { get; init; }

    public byte TypeId { get; init; }

    public string? ReferenceId { get; init; }
}

