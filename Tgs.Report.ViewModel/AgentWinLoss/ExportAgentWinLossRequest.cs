using System;
using System.IO;
using MediatR;

namespace Tgs.Report.ViewModel.AgentWinLoss;

public record ExportAgentWinLossRequest(Guid BrandId, Guid AgentId,
    DateTimeOffset From, DateTimeOffset To) : IRequest<MemoryStream>;