using Tgs.Report.Enum;

namespace Tgs.Report.Core.Queries.Player
{
    public class AdjustRemarkModel
    {
        public AdjustmentTransactionType TransactionType { get; set; }
        public string TargetAgent { get; set; }
        public string Remark { get; set; }
    }
}