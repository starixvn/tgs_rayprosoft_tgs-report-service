using System;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Agent;

public record GetSingleAgentDividendsRequest : PageableRequest, IRequest<PagedResponse<AgentDividendModel>>
{
    public Guid BrandId { get; init; }
    public Guid? AgentId { get; init; }
    public string? Username { get; init; }
    public AgentType? Type { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}