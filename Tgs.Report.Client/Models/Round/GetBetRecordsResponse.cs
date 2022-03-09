namespace Tgs.Report.Client.Models.Round
{
    public record GetBetRecordsResponse
    {
        public int TotalCount { get; init; }

        public BetRecordModel[] Datas { get; init; }

        public BetRecordTotalModel Total { get; init; }
    }
}