using MediatR;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core.Queries.AgentWinLoss;

internal class GetAgentWinLossQueryHandler(IAgentFactory agentFactory) : IRequestHandler<GetAgentWinLossRequest,
    PagedResponse<AgentWinLossModel>>
{
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

        var response = await agentFactory.GetAgentWinLossAsync(getRequest);

        var agentWinLossModels = response as AgentWinLossModel[] ?? response.ToArray();

        return agentWinLossModels.AsQueryable().CreatePagedResponse(request);
    }
}
