namespace Tgs.Report.Client.Models.PlayerDeposit
{
    public record PlayerDepositModel
    {
        public DateTimeOffset ReportDate { get; init; }

        public int FirstDepositCount { get; init; }

        public int NoSecondDepositCount { get; init; }
    }
}