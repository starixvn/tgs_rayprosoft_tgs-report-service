using System;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Player
{
    public record PlayerAccountChangeModel
    {
        public Guid? TransactionId { get; init; }
        public Guid? GameActionId { get; init; }
        public decimal Amount { get; init; }
        public decimal Balance { get; init; }
        public TransactionType? TransactionType { get; init; }
        public GameActionType? GameActionType { get; init; }
        public DateTimeOffset RecordedOn { get; init; }
        public int TotalCount { get; init; }
        public string? Description { get; init; }
    }
}