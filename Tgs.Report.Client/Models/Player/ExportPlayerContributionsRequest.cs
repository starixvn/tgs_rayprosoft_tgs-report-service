using System;

namespace Tgs.Report.Client.Models.Player
{
    public record ExportPlayerContributionsRequest(Guid BrandId, string Username, int? Level, DateTimeOffset From,
        DateTimeOffset To);
}