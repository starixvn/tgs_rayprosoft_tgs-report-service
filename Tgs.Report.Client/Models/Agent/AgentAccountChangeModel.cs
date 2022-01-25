using System;

namespace Tgs.Report.Client.Models.Agent
{
    public record AgentAccountChangeModel
    {
        public Guid TransactionId { get; init; }
        public decimal Amount { get; init; }
        public decimal Balance { get; init; }
        public short TransactionType { get; init; }
        public DateTimeOffset RecordedOn { get; init; }
    }
}