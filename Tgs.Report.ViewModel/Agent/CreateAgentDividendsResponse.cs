namespace Tgs.Report.ViewModel.Agent
{
    public record CreateAgentDividendsResponse : MessageResponse
    {
        public CreateAgentDividendModel[] AgentDividends { get; init; }
    }
}