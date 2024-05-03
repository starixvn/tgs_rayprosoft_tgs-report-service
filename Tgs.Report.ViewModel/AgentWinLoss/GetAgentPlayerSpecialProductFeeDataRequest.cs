using System;

namespace Tgs.Report.ViewModel.AgentWinLoss;

public record GetAgentPlayerSpecialProductFeeDataRequest
{
    public Guid BrandId { get; init; }
    public Guid AgentId { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}