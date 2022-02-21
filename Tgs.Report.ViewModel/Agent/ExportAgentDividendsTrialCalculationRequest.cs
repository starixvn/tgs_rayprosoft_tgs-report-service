using System;
using System.IO;
using MediatR;

namespace Tgs.Report.ViewModel.Agent
{
    public record ExportAgentDividendsTrialCalculationRequest
        (string BrandCode, DateTimeOffset? From, DateTimeOffset? To) : IRequest<MemoryStream>;
}