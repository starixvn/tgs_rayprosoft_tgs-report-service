using System;

namespace Tgs.Report.ViewModel.AgentWinLoss;

public record SpecialDetailDataModel
{
    public DateTimeOffset ReportDate { get; set; }
    public decimal Percentage { get; set; }
    public decimal WinLoss { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal PlatformChargeAmount { get; set; }
}