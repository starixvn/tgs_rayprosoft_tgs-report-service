using MediatR;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core.Queries.AgentWinLoss;

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
            PlatformCharge = request.PlatformCharge,
            From = request.From,
            To = request.To
        };

        var response = await _agentFactory.GetDownLineAgentWinLossAsync(getRequest);

        var agentWinLossModels = response as AgentWinLossModel[] ?? response.ToArray();

        return agentWinLossModels.AsQueryable().CreatePagedResponse(request);
    }
}
