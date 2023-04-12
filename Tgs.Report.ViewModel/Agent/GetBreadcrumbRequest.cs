using System;
using System.Collections.Generic;
using MediatR;

namespace Tgs.Report.ViewModel.Agent;

public record GetBreadcrumbRequest : IRequest<IEnumerable<BreadcrumbModel>>
{
    public Guid? AgentId { get; init; }
    public string? PlayerCode { get; init; }
    public Guid? RoundId { get; init; }
}