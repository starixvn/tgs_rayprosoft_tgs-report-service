using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent;

public class
    GetSingleAgentDividendQueryHandler(TgsContext context) : IRequestHandler<GetSingleAgentDividendsRequest,
    PagedResponse<AgentDividendModel>>
{
    private readonly TgsContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<PagedResponse<AgentDividendModel>> Handle(
        GetSingleAgentDividendsRequest request,
        CancellationToken cancellationToken)
    {
        var agent = await _context.Agents.Where(x => x.BrandId == request.BrandId)
            .Where(request.Username, x => request.Username == x.Username)
            .Where(request.AgentId, x => x.Id == request.AgentId)
            .Select(x => new
            {
                x.Id,
                x.Username
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (agent is null)
        {
            return new()
            {
                Valid = false,
                Message = Resources.Agent.NotFound
            };
        }

        var agentDividends = await _context.AgentDividends
            .Include(x => x.AgentDividendMultipleLayers)
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.From >= request.From.ToUniversalTime())
            .Where(x => x.To < request.To.ToUniversalTime())
            .Where(x => agent.Id == x.AgentId)
            .GroupBy(x => new { x.AgentDividendPercentage },
                (x, list) => new
                {
                    AgentId = agent.Id,
                    x.AgentDividendPercentage,
                    CompanyAdjustAmount = list.Sum(l => l.CompanyAdjustAmount),
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

        var agentCovered = await _context.AgentDividendMultipleLayers
            .Include(x => x.AgentDividend)
            .Where(x => x.AgentDividend.BrandId == request.BrandId)
            .Where(x => x.AgentDividend.From >= request.From.ToUniversalTime())
            .Where(x => x.AgentDividend.To < request.To.ToUniversalTime())
            .Where(x => agent.Id == x.AgentId)
            .Where(x => x.AgentId == x.DividendProviderAgentId)
            .Where(x => x.AgentCoveredBonus != null || x.AgentCoveredBonusAdjust != null)
            .GroupBy(x => new
            {
                x.AgentId,
                x.AgentDividendPercentage
            }, (agent, list) => new
            {
                agent.AgentId,
                agent.AgentDividendPercentage,
                AgentCoveredBonus = list.Sum(l => l.AgentCoveredBonus),
                AgentCoveredBonusAdjust = list.Sum(l => l.AgentCoveredBonusAdjust)
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var fees = await context.AgentDividends
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.AgentId == agent.Id)
            .Where(x => x.From >= request.From.ToUniversalTime())
            .Where(x => x.To < request.To.ToUniversalTime())
            .GroupJoin(context.AgentDividendProductFees,
                a => a.Id,
                a => a.AgentDividendId,
                (agentDividend, productFees) => new { agentDividend, productFees })
            .GroupBy(x => new { x.agentDividend.AgentId, x.agentDividend.AgentDividendPercentage }, (agent, fees) => new
            {
                agent.AgentId,
                agent.AgentDividendPercentage,
                CardGameAmount =
                    fees.SelectMany(s => s.productFees)
                        .Sum(s => s.CardGameWinLossAmount <= 0M ? 0M : s.CardGameWinLossAmount * s.CardGamePercentage / 100),
                LiveDealerAmount =
                    fees.SelectMany(s => s.productFees)
                        .Sum(s => s.LiveDealerWinLossAmount <= 0M ? 0M : s.LiveDealerWinLossAmount * s.LiveDealerPercentage / 100),
                LotteryAmount =
                    fees.SelectMany(s => s.productFees)
                        .Sum(s => s.LotteryWinLossAmount <= 0M ? 0M : s.LotteryWinLossAmount * s.LotteryPercentage / 100),
                SportsAmount =
                    fees.SelectMany(s => s.productFees)
                        .Sum(s => s.SportsWinLossAmount <= 0M ? 0M : s.SportsWinLossAmount * s.SportsPercentage / 100),
                SlotAmount =
                    fees.SelectMany(s => s.productFees)
                        .Sum(s => s.SlotWinLossAmount <= 0M ? 0M : s.SlotWinLossAmount * s.SlotPercentage / 100),
                VirtualSportsAmount =
                    fees.SelectMany(s => s.productFees)
                        .Sum(s => s.VirtualSportsWinLossAmount <= 0M ? 0M : s.VirtualSportsWinLossAmount * s.VirtualSportsPercentage / 100),
            }).ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var models = agentDividends.Select(
                ad => new
                {
                    Id = ad.AgentId,
                    Username = agent.Username,
                    CompanyAdjustAmount = ad.CompanyAdjustAmount,
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
                    AgentDividendPercentage = ad.AgentDividendPercentage
                })
            .GroupJoin(agentCovered,
                x => new { AgentId = x.Id, x.AgentDividendPercentage },
                c => new { c.AgentId, c.AgentDividendPercentage },
                (ad, list) => new { ad, list })
            .SelectMany(x => x.list.DefaultIfEmpty(), (x, covered) => new { x.ad, covered })
            .Select(x => new
            {
                Id = x.ad.Id,
                Username = x.ad.Username,
                CompanyAdjustAmount = x.ad.CompanyAdjustAmount,
                AgentAdjustAmount = x.ad.AgentAdjustAmount,
                BonusAdjustAmount = x.ad.BonusAdjustAmount,
                BonusAmount = x.ad.BonusAmount,
                BetAmount = x.ad.BetAmount,
                ValidBetAmount = x.ad.ValidBetAmount,
                WinLossAmount = x.ad.WinLossAmount,
                NetProfit = x.ad.NetProfit,
                AgentDividendAmount = x.ad.AgentDividendAmount,
                FromPlayerAgentDividendAmount = x.ad.FromPlayerAgentDividendAmount,
                FromTeamAgentDividendAmount = x.ad.FromTeamAgentDividendAmount,
                AgentDividendPercentage = x.ad.AgentDividendPercentage,
                AgentCoveredBonus = x.covered?.AgentCoveredBonus,
                AgentCoveredBonusAdjust = x.covered?.AgentCoveredBonusAdjust
            })
            .GroupJoin(fees,
                x => new { x.Id, x.AgentDividendPercentage },
                c => new { Id = c.AgentId, c.AgentDividendPercentage },
                (ad, list) => new { ad, list })
            .SelectMany(x => x.list.DefaultIfEmpty(), (x, fee) => new { x.ad, fee })
            .Select(x => new AgentDividendModel
            {
                Id = x.ad.Id,
                Username = x.ad.Username,
                CompanyAdjustAmount = x.ad.CompanyAdjustAmount,
                AgentAdjustAmount = x.ad.AgentAdjustAmount,
                BonusAdjustAmount = x.ad.BonusAdjustAmount,
                BonusAmount = x.ad.BonusAmount,
                BetAmount = x.ad.BetAmount,
                ValidBetAmount = x.ad.ValidBetAmount,
                WinLossAmount = x.ad.WinLossAmount,
                NetProfit = x.ad.NetProfit,
                AgentDividendAmount = x.ad.AgentDividendAmount - (x.fee == null
                    ? 0M
                    : x.fee.CardGameAmount + x.fee.LiveDealerAmount + x.fee.LotteryAmount + x.fee.SlotAmount +
                      x.fee.SportsAmount + x.fee.VirtualSportsAmount),
                FromPlayerAgentDividendAmount = x.ad.FromPlayerAgentDividendAmount,
                FromTeamAgentDividendAmount = x.ad.FromTeamAgentDividendAmount,
                AgentDividendPercentage = x.ad.AgentDividendPercentage,
                AgentCoveredBonus = x.ad.AgentCoveredBonus,
                AgentCoveredBonusAdjust = x.ad.AgentCoveredBonusAdjust,
                ProductFee = x.fee == null
                    ? 0M
                    : x.fee.CardGameAmount + x.fee.LiveDealerAmount + x.fee.LotteryAmount + x.fee.SlotAmount +
                      x.fee.SportsAmount + x.fee.VirtualSportsAmount
            })
            .ToArray();

        return models.AsQueryable().CreatePagedResponse(request);
    }
}