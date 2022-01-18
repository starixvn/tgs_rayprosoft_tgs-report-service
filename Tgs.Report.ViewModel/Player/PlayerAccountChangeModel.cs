using System;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Player
{
    public class PlayerAccountChangeModel
    {
        public Guid? TransactionId { get; set; }
        public Guid? GameActionId { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public TransactionType? TransactionType { get; set; }
        public GameActionType? GameActionType { get; set; }
        public DateTimeOffset RecordedOn { get; set; }
        public int TotalCount { get; set; }
    }
}