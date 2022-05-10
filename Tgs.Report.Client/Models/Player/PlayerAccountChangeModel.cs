using System;

namespace Tgs.Report.Client.Models.Player
{
    public record PlayerAccountChangeModel
    {
        public Guid? TransactionId { get; init; }
        public Guid? GameActionId { get; init; }
        public decimal Amount { get; init; }
        public decimal Balance { get; init; }
        public short? TransactionType { get; init; }
        public short? GameActionType { get; init; }
        public DateTimeOffset RecordedOn { get; init; }

        public string? Description { get; init; }
    }
}