using MediatR;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Infrastructures.Extensions.Models;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core.Queries.AgentWinLoss;

internal class GetMasterAgentWinLossQueryHandler(IAgentFactory agentFactory)
    : IRequestHandler<GetMasterAgentWinLossRequest,
        PagedResponse<AgentWinLossModel>>
{
    public async Task<PagedResponse<AgentWinLossModel>> Handle(
        GetMasterAgentWinLossRequest request,
        CancellationToken cancellationToken)
    {
        var getRequest = new GetMasterAgentWinLossDataRequest
        {
            BrandId = request.BrandId,
            From = request.From,
            To = request.To
        };

        var response = await agentFactory.GetMasterAgentWinLossAsync(getRequest);

        var agentWinLossModels = response as AgentWinLossModel[] ?? response.ToArray();

        return agentWinLossModels.AsQueryable().CreatePagedResponse(request);
    }
}
