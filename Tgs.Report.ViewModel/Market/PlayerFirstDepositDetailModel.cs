using System;

namespace Tgs.Report.ViewModel.Market
{
    public record PlayerFirstDepositDetailModel
    {
        public string Username { get; init; } = default!;
        public DateTimeOffset FirstDepositOn { get; init; }
        public int FirstDepositAmount { get; init; }
        public int TotalDepositAmount { get; init; }
        public int TotalWithdrawAmount { get; init; }
        public int DepositSubtractWithdraw { get; init; }
        public string IpAddress { get; init; } = default!;
    }
}