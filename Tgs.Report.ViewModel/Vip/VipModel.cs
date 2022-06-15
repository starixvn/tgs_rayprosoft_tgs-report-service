using System;

namespace Tgs.Report.ViewModel.Vip
{
    public record VipModel
    {
        public Guid BrandId { get; init; }

        public string? Username { get; init; }

        public string PlayerCode { get; init; } = default!;

        public int? Level { get; init; }

        public decimal DepositAmount { get; init; }

        public decimal WithdrawalAmount { get; set; }

        public decimal BonusAmount { get; init; }

        public decimal CompanyAdjustAmount { get; init; }

        public decimal BonusAdjustAmount { get; init; }

        public decimal RebateAmount { get; init; }

        public decimal BetAmount { get; init; }

        public decimal ValidBetAmount { get; init; }

        public decimal Turnover { get; init; }

        public decimal WinLossAmount { get; init; }

        public decimal NetProfit { get; init; }
    }
}