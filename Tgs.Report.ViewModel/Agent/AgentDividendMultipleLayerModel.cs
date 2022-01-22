using System;

namespace Tgs.Report.ViewModel.Agent
{
    public record AgentDividendMultipleLayerModel
    {
        public Guid AgentId { get; init; }
        public Guid DividendProviderAgentId { get; init; }
        public string Layer { get; init; } = default!;
        public decimal Dividend { get; init; }
    }
}