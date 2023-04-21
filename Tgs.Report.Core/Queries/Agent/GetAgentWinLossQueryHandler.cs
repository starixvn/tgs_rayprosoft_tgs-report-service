using MediatR;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent;

internal class GetAgentWinLossQueryHandler : IRequestHandler<GetAgentWinLossRequest,
    PagedResponse<AgentWinLossModel>>
{
    private readonly IAgentFactory _agentFactory;

    public GetAgentWinLossQueryHandler(IAgentFactory agentFactory)
    {
        _agentFactory = agentFactory;
    }

    public async Task<PagedResponse<AgentWinLossModel>> Handle(
        GetAgentWinLossRequest request,
        CancellationToken cancellationToken)
    {
        var getRequest = new GetAgentWinLossDataRequest
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            From = request.From,
            To = request.To
        };

        return (await _agentFactory.GetAgentWinLossAsync(getRequest))
            .AsQueryable().CreatePagedResponse(request);
    }
}
