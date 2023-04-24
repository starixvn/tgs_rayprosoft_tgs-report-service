using System;
using System.Collections.Generic;
using MediatR;

namespace Tgs.Report.ViewModel.AgentWinLoss;

public record GetAgentSpecialDetailRequest : IRequest<IEnumerable<AgentSpecialDetailModel>>
{
    public Guid BrandId { get; init; }
    public Guid AgentId { get; init; }
    public decimal PlatformCharge { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}