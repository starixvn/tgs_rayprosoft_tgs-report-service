using MediatR;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent;

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
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            ShowAll = request.ShowAll,
            SortBy = request.SortBy,
        };

        var list = await _agentFactory.GetAgentWinLossAsync(getRequest);

        return new PagedResponse<AgentWinLossModel>
        {
            Valid = true,
            Items = list.Select(x => new AgentWinLossModel
            {
                Id = x.Id,
                Username = x.Username,
                MemberType = x.MemberType,
                Turnover = x.Turnover,
                WinLoss = x.WinLoss,
                AgentWinLoss = x.AgentWinLoss,
                CompanyWinLoss = x.CompanyWinLoss,
                Special = x.Special,
                AgentSpecial = x.AgentSpecial,
                CompanySpecial = x.CompanySpecial
            }).ToArray(),
            TotalCount = list.FirstOrDefault()?.TotalCount ?? 0,
        };
    }
}
