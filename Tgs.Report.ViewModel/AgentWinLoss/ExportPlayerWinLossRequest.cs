using System;
using System.IO;
using MediatR;

namespace Tgs.Report.ViewModel.AgentWinLoss;

public record ExportPlayerWinLossRequest(string PlayerCode,
    DateTimeOffset From, DateTimeOffset To) : IRequest<MemoryStream>;