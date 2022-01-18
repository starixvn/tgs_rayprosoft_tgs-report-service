using System;
using Tgs.Report.Enum;

namespace Tgs.Report.Client.Models.Agent
{
    public class AgentAccountChangeModel
    {
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTimeOffset RecordedOn { get; set; }
    }
}