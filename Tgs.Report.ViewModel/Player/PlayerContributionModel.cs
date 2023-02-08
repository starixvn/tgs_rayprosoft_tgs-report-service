using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Player;

public record PlayerContributionModel
{
    public string Username { get; set; } = default!;

    public string PlayerCode { get; init; } = default!;

    public AccountType Type { get; init; }

    public int? Level { get; set; }

    public decimal DepositAmount { get; set; }

    public decimal DepositHandlingFee { get; set; }

    public decimal WithdrawalAmount { get; set; }

    public decimal CompanyAdjustAmount { get; init; }

    public decimal AgentAdjustAmount { get; init; }

    public decimal BonusAdjustAmount { get; init; }

    public decimal BonusAmount { get; init; }

    public decimal BetAmount { get; init; }

    public decimal ValidBetAmount { get; init; }

    public decimal Turnover { get; init; }

    public decimal WinLossAmount { get; init; }

    public decimal NetProfit { get; init; }
}