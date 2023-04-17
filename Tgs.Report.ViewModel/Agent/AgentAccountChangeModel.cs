using System;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Agent;

public record AgentAccountChangeModel
{
    public Guid TransactionId { get; init; }
    public decimal Amount { get; init; }
    public decimal Balance { get; init; }
    public TransactionType TransactionType { get; init; }
    public DateTimeOffset RecordedOn { get; init; }
}