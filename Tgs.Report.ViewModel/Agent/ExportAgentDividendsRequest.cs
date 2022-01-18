using System;
using System.IO;
using MediatR;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Agent
{
    public record ExportAgentDividendsRequest(Guid BrandId, Guid? AgentId, string Username, AgentType? Type,
        DateTimeOffset From, DateTimeOffset To) : IRequest<MemoryStream>;
}