using System;
using System.IO;
using MediatR;

namespace Tgs.Report.ViewModel.Player
{
    public record ExportPlayerContributionsRequest(Guid BrandId, string Username, int? Level, DateTimeOffset From,
        DateTimeOffset To) : IRequest<MemoryStream>;
}