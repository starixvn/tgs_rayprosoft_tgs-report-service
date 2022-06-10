using System;
using System.IO;
using MediatR;

namespace Tgs.Report.ViewModel.Vip
{
    public record ExportVipReportRequest(Guid BrandId, string? Username, int? Level, DateTimeOffset From,
        DateTimeOffset To) : IRequest<MemoryStream>;
}