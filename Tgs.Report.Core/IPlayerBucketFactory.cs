using Tgs.Report.ViewModel;

namespace Tgs.Report.Core
{
    public interface IPlayerBucketFactory
    {
        ValueTask FillInBucketAsync(FillInPlayerBucketsRequest request);
    }
}