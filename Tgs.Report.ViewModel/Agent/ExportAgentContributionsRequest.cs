using System;
using System.IO;
using MediatR;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Agent;

public record ExportAgentContributionsRequest(Guid BrandId, Guid? AgentId, string Username, AgentType? Type,
    DateTimeOffset From, DateTimeOffset To, bool IncludeAgents) : IRequest<MemoryStream>;