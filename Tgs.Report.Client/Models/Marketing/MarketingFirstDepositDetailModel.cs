namespace Tgs.Report.Client.Models.Marketing
{
    public record MarketingFirstDepositDetailModel
    {
        public string Username { get; init; } = default!;
        public DateTimeOffset FirstDepositOn { get; init; }
        public decimal FirstDepositAmount { get; init; }
        public decimal TotalDepositAmount { get; init; }
        public decimal TotalWithdrawAmount { get; init; }
        public decimal DepositSubtractWithdraw { get; init; }
        public string IpAddress { get; init; } = default!;
    }
}