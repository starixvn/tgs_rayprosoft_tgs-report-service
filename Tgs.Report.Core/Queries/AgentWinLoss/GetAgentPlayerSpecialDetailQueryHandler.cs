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
        var response = await agentFactory.GetAgentPlayerSpecialFeesAsync(new GetAgentPlayerSpecialFeesDataRequest
        {
            BrandId = request.BrandId, AgentId = request.AgentId, From = request.From, To = request.To
        });

        return new AgentPlayerSpecialDetailModel
        {
            Bonus = response.Bonus,
            BonusAdjust = response.BonusAdjust,
            CompanyAdjust = response.CompanyAdjust,
            AgentAdjust = response.AgentAdjust
        };
    }
}
