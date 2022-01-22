using System;

namespace Tgs.Report.ViewModel.Agent
{
    public record AgentDividendsTrialCalculationModel
    {
        public string AgentUsername { get; init; } = default!;
        public Guid AgentId { get; init; }
        public string BrandCode { get; init; } = default!;
        public Guid BrandId { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
        public decimal AgentDividendPercentage { get; init; }
        public decimal AgentDividendAmount { get; init; }
        public decimal NegativeDividendAmount { get; init; }
        public bool AutoSend { get; init; }
    }
}