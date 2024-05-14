using System;
using System.IO;
using MediatR;

namespace Tgs.Report.ViewModel.AgentWinLoss;

public record ExportMasterAgentWinLossRequest(Guid BrandId,
    DateTimeOffset From, DateTimeOffset To) : IRequest<MemoryStream>;