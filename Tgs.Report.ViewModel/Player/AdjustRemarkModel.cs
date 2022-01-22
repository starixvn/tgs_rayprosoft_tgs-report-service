using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Player
{
    public class AdjustRemarkModel
    {
        public AdjustmentTransactionType TransactionType { get; init; }
        public string? TargetAgent { get; init; }
        public string? Remark { get; init; }
    }
}