using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent;

public class GetAgentDividendQueryHandler : IRequestHandler<GetAgentDividendsRequest, PagedResponse<AgentDividendModel>>
{
    private readonly TgsContext _context;

    public GetAgentDividendQueryHandler(TgsContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<PagedResponse<AgentDividendModel>> Handle(
        GetAgentDividendsRequest request,
        CancellationToken cancellationToken)
    {
        var mainAgent = await _context.Agents.Where(x => x.BrandId == request.BrandId)
            .Where(request.Username, x => request.Username == x.Username)
            .Where(request.AgentId, x => x.Id == request.AgentId)
            .Select(x => new
            {
                x.Id,
                x.Username
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (mainAgent is null)
        {
            return new()
            {
                Valid = false,
                Message = Resources.Agent.NotFound
            };
        }

        var subAgents = await _context.Agents.Where(x => x.ParentAgentId == mainAgent.Id)
            .Select(x => new
            {
                x.Id,
                x.Username
            })
            .ToListAsync(cancellationToken);

        var agents = subAgents.Union(new[]
        {
            mainAgent
        }).ToArray();

        var agentIds = agents.Select(x => x.Id).ToArray();

        var agentDividends = await _context.AgentDividends
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.From >= request.From.ToUniversalTime())
            .Where(x => x.To < request.To.ToUniversalTime())
            .Where(x => agentIds.Contains(x.AgentId))
            .Select(x => new
            {
                x.AgentId,
                x.DepositAmount,
                x.WithdrawalAmount,
                x.CompanyAdjustAmount,
                x.AgentAdjustAmount,
                x.BonusAmount,
                x.BetAmount,
                x.ValidBetAmount,
                x.OriginalTurnover,
                x.Turnover,
                x.WinLossAmount,
                x.NetProfit,
                x.AgentDividendAmount,
                x.BeforeDividendAmount,
                x.NoIncludeNetProfitAgentAdjustAmount,
                x.ReferencedAgentDividendId,
                x.SentOn,
                x.AgentDividendMultipleLayers,
                x.BonusAdjustAmount,
                x.NoIncludeNetProfitBonusAdjustAmount,
                x.AgentDividendPercentage
            })
            .GroupBy(x => new
                {
                    x.AgentId,
                    x.AgentDividendPercentage
                },
                (x, list) => new
                {
                    x.AgentId,
                    x.AgentDividendPercentage,
                    AgentAdjustAmount = list.Sum(l => l.AgentAdjustAmount),
                    BonusAdjustAmount = list.Sum(l => l.BonusAdjustAmount),
                    BonusAmount = list.Sum(l => l.BonusAmount),
                    BetAmount = list.Sum(l => l.BetAmount),
                    OriginalTurnover = list.Sum(l => l.OriginalTurnover),
                    WinLossAmount = list.Sum(l => l.WinLossAmount),
                    NetProfit = list.Sum(l => l.NetProfit),
                    AgentDividendAmount = list.Sum(l => l.BeforeDividendAmount ?? l.AgentDividendAmount),
                    NoIncludeNetProfitAgentAdjustAmount = list.Sum(l => l.NoIncludeNetProfitAgentAdjustAmount),
                    NoIncludeNetProfitBonusAdjustAmount = list.Sum(l => l.NoIncludeNetProfitBonusAdjustAmount),
                    FromPlayerAgentDividendAmount =
                        list.Sum(l => l.AgentDividendMultipleLayers.Where(m => m.AgentId == m.DividendProviderAgentId)
                            .Sum(s => s.Dividend)),
                    FromTeamAgentDividendAmount = list.Sum(l
                        => l.AgentDividendMultipleLayers.Where(m => m.AgentId != m.DividendProviderAgentId)
                            .Sum(s => s.Dividend))
                })
            .ToArrayAsync(cancellationToken);

        var models = agentDividends.Join(agents,
            ad => ad.AgentId,
            a => a.Id,
            (ad, a) => new AgentDividendModel
            {
                Id = ad.AgentId,
                Username = a.Username,
                AgentAdjustAmount = ad.AgentAdjustAmount - ad.NoIncludeNetProfitAgentAdjustAmount,
                BonusAdjustAmount = ad.BonusAdjustAmount - ad.NoIncludeNetProfitBonusAdjustAmount,
                BonusAmount = ad.BonusAmount,
                BetAmount = ad.BetAmount,
                ValidBetAmount = ad.OriginalTurnover,
                WinLossAmount = ad.WinLossAmount,
                NetProfit = ad.NetProfit,
                AgentDividendAmount = ad.AgentDividendAmount,
                FromPlayerAgentDividendAmount = ad.FromPlayerAgentDividendAmount,
                FromTeamAgentDividendAmount = ad.FromTeamAgentDividendAmount,
                AgentDividendPercentage = ad.AgentDividendPercentage,
                AgentCoveredBonus = ad.BonusAmount * ad.AgentDividendPercentage,
                AgentCoveredBonusAdjust = ad.BonusAdjustAmount * ad.AgentDividendPercentage
            }).ToArray();

        return models.AsQueryable().CreatePagedResponse(request);
    }
}