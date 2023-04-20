using MediatR;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent;

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
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll,
        };

        var list = await _agentFactory.GetMasterAgentWinLossAsync(getRequest);

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
