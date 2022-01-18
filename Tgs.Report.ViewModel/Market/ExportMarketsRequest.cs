using System;
using System.IO;
using MediatR;

namespace Tgs.Report.ViewModel.Market
{
    public record ExportMarketsRequest(Guid BrandId, DateTimeOffset From, DateTimeOffset To) : IRequest<MemoryStream>;
}