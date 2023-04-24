using MediatR;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core.Queries.AgentWinLoss;

internal class GetMasterAgentWinLossQueryHandler : IRequestHandler<GetMasterAgentWinLossRequest,
    PagedResponse<AgentWinLossModel>>
{
    private readonly IAgentFactory _agentFactory;

    public GetMasterAgentWinLossQueryHandler(IAgentFactory agentFactory)
    {
        _agentFactory = agentFactory;
    }

    public async Task<PagedResponse<AgentWinLossModel>> Handle(
        GetMasterAgentWinLossRequest request,
        CancellationToken cancellationToken)
    {
        var getRequest = new GetMasterAgentWinLossDataRequest
        {
            BrandId = request.BrandId,
            PlatformCharge = request.PlatformCharge,
            From = request.From,
            To = request.To
        };

        return (await _agentFactory.GetMasterAgentWinLossAsync(getRequest))
            .AsQueryable().CreatePagedResponse(request);
    }
}
