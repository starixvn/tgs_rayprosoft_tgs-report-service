namespace Tgs.Report.Client.Models.Agent;

public record GetAgentTeamInfoRequest : PageableRequest
{
    public Guid AgentId { get; init; }
}