namespace Tgs.Report.Client.Models.Marketing
{
    public record MarketingFirstDepositDetailModel
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