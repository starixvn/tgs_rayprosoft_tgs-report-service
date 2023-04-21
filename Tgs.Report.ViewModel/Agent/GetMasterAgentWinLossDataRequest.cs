using System;

namespace Tgs.Report.ViewModel.Agent;

public record GetMasterAgentWinLossDataRequest
{
    public Guid BrandId { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}