namespace Tgs.Report.ViewModel.Round;

public record GetBetRecordsResponse
{
    public int TotalCount { get; init; }

    public BetRecordModel[] Datas { get; init; } = default!;

    public BetRecordTotalModel Total { get; init; } = default!;
}