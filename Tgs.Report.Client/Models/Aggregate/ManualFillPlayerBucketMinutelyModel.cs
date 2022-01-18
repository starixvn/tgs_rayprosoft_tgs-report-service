namespace Tgs.Report.Client.Models.Aggregate
{
    public record ManualFillPlayerBucketMinutelyModel(string Date, int Hour, int FromMinute, int ToMinute,
        bool FillPlayerBucket);
}