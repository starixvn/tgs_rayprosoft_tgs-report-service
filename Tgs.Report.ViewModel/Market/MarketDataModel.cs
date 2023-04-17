using System;

namespace Tgs.Report.ViewModel.Market;

public record MarketDataModel
{
    public Guid BrandId { get; init; }
    public string PlayerCode { get; init; } = default!;
    public int Hour { get; init; }
    public DateTimeOffset CreatedOn { get; init; }
    public DateTimeOffset ReportDate { get; init; }
    public int RegisteredCount { get; init; }
    public int FirstFundInCount { get; init; }
    public decimal DepositAmount { get; init; }
    public decimal WithdrawalAmount { get; init; }
    public decimal AdjustmentAmount { get; init; }
    public decimal BonusAmount { get; init; }
    public decimal BetAmount { get; init; }
    public decimal Turnover { get; init; }
    public decimal WinLossAmount { get; init; }
    public decimal NetProfit { get; init; }
}