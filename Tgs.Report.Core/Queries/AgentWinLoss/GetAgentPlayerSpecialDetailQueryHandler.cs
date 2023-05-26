using MediatR;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core.Queries.AgentWinLoss;

internal class GetAgentPlayerSpecialDetailQueryHandler : IRequestHandler<GetAgentPlayerSpecialDetailRequest,
    AgentPlayerSpecialDetailModel>
{
    private readonly IAgentFactory _agentFactory;

    public GetAgentPlayerSpecialDetailQueryHandler(IAgentFactory agentFactory)
    {
        _agentFactory = agentFactory;
    }

    public async Task<AgentPlayerSpecialDetailModel> Handle(
        GetAgentPlayerSpecialDetailRequest request,
        CancellationToken cancellationToken)
    {
        var getRequest = new GetAgentPlayerSpecialDetailDataRequest
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            PlatformCharge = request.PlatformCharge,
            From = request.From,
            To = request.To
        };
        var response = await _agentFactory.GetAgentPlayerSpecialDetailAsync(getRequest);

        return response.FirstOrDefault() ?? new AgentPlayerSpecialDetailModel();
    }
}
