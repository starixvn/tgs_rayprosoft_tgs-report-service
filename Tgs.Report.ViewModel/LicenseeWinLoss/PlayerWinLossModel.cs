using System;

namespace Tgs.Report.ViewModel.LicenseeWinLoss;

public record PlayerWinLossModel
{
    //public Guid GameProviderId { get; init; }

    public string? GameProviderCode { get; init; }

    public string? WinLossDate { get; set; }

    public decimal PlayerWinLoss {  get; init; }

    //public string? GameProviderCurrencyCode { get; init; }

    public byte TypeId { get; init; }

    //public string? GameCode { get; init; }

    public string? ReferenceId { get; init; }
}

