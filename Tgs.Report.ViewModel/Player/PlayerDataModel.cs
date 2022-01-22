using System;

namespace Tgs.Report.ViewModel.Player
{
    public record PlayerDataModel
    {
        public Guid BrandId { get; init; }
        public Guid? AgentId { get; init; }
        public string PlayerCode { get; init; } = default!;
        public int Hour { get; init; }
        public DateTimeOffset CreatedOn { get; init; }
        public DateTimeOffset ReportDate { get; init; }
        public decimal Contribution { get; init; }
    }
}