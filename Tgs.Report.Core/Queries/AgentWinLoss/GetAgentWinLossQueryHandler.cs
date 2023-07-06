using MediatR;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core.Queries.AgentWinLoss;

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
            PlatformCharge = request.PlatformCharge,
            From = request.From,
            To = request.To
        };

        var response = await _agentFactory.GetAgentWinLossAsync(getRequest);

        var agentWinLossModels = response as AgentWinLossModel[] ?? response.ToArray();

        return agentWinLossModels.AsQueryable().CreatePagedResponse(request);
    }
}
