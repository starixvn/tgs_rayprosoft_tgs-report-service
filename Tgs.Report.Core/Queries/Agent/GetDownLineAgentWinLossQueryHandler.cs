using MediatR;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent;

internal class GetDownLineAgentWinLossQueryHandler : IRequestHandler<GetDownLineAgentWinLossRequest,
    PagedResponse<AgentWinLossModel>>
{
    private readonly IAgentFactory _agentFactory;

    public GetDownLineAgentWinLossQueryHandler(IAgentFactory agentFactory)
    {
        _agentFactory = agentFactory;
    }

    public async Task<PagedResponse<AgentWinLossModel>> Handle(
        GetDownLineAgentWinLossRequest request,
        CancellationToken cancellationToken)
    {
        var getRequest = new GetDownLineAgentWinLossDataRequest
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            From = request.From,
            To = request.To
        };

        return (await _agentFactory.GetDownLineAgentWinLossAsync(getRequest))
            .AsQueryable().CreatePagedResponse(request);
    }
}
