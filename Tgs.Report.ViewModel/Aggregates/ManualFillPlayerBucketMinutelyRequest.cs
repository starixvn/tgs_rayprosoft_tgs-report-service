using MediatR;

namespace Tgs.Report.ViewModel.Aggregates
{
    public record ManualFillPlayerBucketMinutelyRequest
        (string Date, int Hour, int FromMinute, int ToMinute, bool FillPlayerBucket) : IRequest<MessageResponse>;
}