using MediatR;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core.Queries.AgentWinLoss;

internal class GetAgentPlayerSpecialDetailQueryHandler : IRequestHandler<GetAgentPlayerSpecialDetailRequest,
    IEnumerable<AgentPlayerSpecialDetailModel>>
{
    private readonly IAgentFactory _agentFactory;

    public GetAgentPlayerSpecialDetailQueryHandler(IAgentFactory agentFactory)
    {
        _agentFactory = agentFactory;
    }

    public Task<IEnumerable<AgentPlayerSpecialDetailModel>> Handle(
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

        return _agentFactory.GetAgentPlayerSpecialDetailAsync(getRequest);
    }
}
