using System;
using System.Collections.Generic;
using MediatR;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.ViewModel.AgentWinLoss;

public record GetBreadcrumbRequest : IRequest<IEnumerable<BreadcrumbModel>>
{
    public Guid? AgentId { get; init; }
    public string? PlayerCode { get; init; }
    public Guid? RoundId { get; init; }
}