using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent;

internal class GetAgentWinLossQueryHandler : IRequestHandler<GetAgentWinLossRequest,
    PagedResponse<AgentWinLossModel>>
{
    private readonly TgsContext _context;
    private readonly IAgentFactory _agentFactory;

    public GetAgentWinLossQueryHandler(TgsContext context, IAgentFactory agentFactory)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _agentFactory = agentFactory;
    }


    public async Task<PagedResponse<AgentWinLossModel>> Handle(
        GetAgentWinLossRequest request,
        CancellationToken cancellationToken)
    {
        var agent = await _context.Agents
            .Select(x => new { x.Id })
            .FirstOrDefaultAsync(x => x.Id == request.AgentId, cancellationToken);

        if (agent == null)
        {
            return new()
            {
                Valid = true
            };
        }
        var list = await _agentFactory.GetAgentWinLossAsync(request);

        //var playerAggregate = _context.Database
        //    .SqlQueryRaw<AgentWinLoss>($@"select brand_id as ""BrandId"", '$[*] ? (@.AgentId == ""{request.AgentId}"").DividendPercentage' ""AgentPercentage"" from report.player_aggregates where jsonb_path_exists(agent_dividend_percentage_tree_data, '$[*] ? (@.AgentId == ""{request.AgentId}"")')")
        //    .ToArray();

        return new PagedResponse<AgentWinLossModel>
        {
            Valid = true,
            Items = list.Select(x => new  AgentWinLossModel
            {
                Username = x.Username,
                MemberType = x.MemberType,
                Turnover = x.Turnover,
                WinLoss = x.WinLoss,
                AgentWinLoss = x.AgentWinLoss,
                CompanyWinLoss = x.CompanyWinLoss
            }).ToArray(),
            TotalCount = list.FirstOrDefault().TotalCount,
        };
    }
}
