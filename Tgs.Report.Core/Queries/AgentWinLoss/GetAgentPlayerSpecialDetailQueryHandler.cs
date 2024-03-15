using MediatR;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core.Queries.AgentWinLoss;

internal class GetAgentPlayerSpecialDetailQueryHandler(IAgentFactory agentFactory)
    : IRequestHandler<GetAgentPlayerSpecialDetailRequest,
        AgentPlayerSpecialDetailModel>
{
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
        var response = await agentFactory.GetAgentPlayerSpecialDetailAsync(getRequest);

        return response.FirstOrDefault() ?? new AgentPlayerSpecialDetailModel();
    }
}
