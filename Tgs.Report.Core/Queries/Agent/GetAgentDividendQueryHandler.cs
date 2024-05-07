using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent;

public class GetAgentDividendQueryHandler(TgsContext context)
    : IRequestHandler<GetAgentDividendsRequest, PagedResponse<AgentDividendModel>>
{
    public async Task<PagedResponse<AgentDividendModel>> Handle(
        GetAgentDividendsRequest request,
        CancellationToken cancellationToken)
    {
        var mainAgent = await context.Agents.Where(x => x.BrandId == request.BrandId)
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
            return new PagedResponse<AgentDividendModel>
            {
                Valid = false,
                Message = Resources.Agent.NotFound
            };
        }

        var offset = new TimeSpan(8, 0, 0);
        var requestFrom = request.From.ToOffset(offset);
        var requestTo = request.To.ToOffset(offset);

        var from = new DateTimeOffset(requestFrom.Year,
            requestFrom.Month,
            requestFrom.Day,
            0,
            0,
            0,
            requestFrom.Offset);

        var to = new DateTimeOffset(requestTo.Year,
            requestTo.Month,
            requestTo.Day,
            0,
            0,
            0,
            requestTo.Offset);

        var now = DateTimeOffset.Now.ToOffset(offset);
        var today = new DateTimeOffset(now.Year,
            now.Month,
            now.Day,
            0,
            0,
            0,
            offset);

        var fromDate = new DateOnly(from.Year, from.Month, from.Day);
        var toDate = new DateOnly(to.Year, to.Month, to.Day);

        var dailyDepositWithdraws = await context.PlayerDailyPaymentStatistics
            .Join(context.Players.Where(x => x.BrandId == request.BrandId), pp => pp.PlayerCode, p => p.PlayerCode,
                (pp, p) => new
                {
                    pp.ReportDate,
                    p.AgentId,
                    pp.DailyDepositAmount,
                    pp.DailyWithdrawAmount
                })
            .Where(x => x.ReportDate >= fromDate && x.ReportDate < toDate)
            .GroupBy(x => x.AgentId, (agentId, group) => new
            {
                AgentId = agentId,
                DailyDepositAmount = group.Sum(s => s.DailyDepositAmount),
                DailyWithdrawAmount = group.Sum(s => s.DailyWithdrawAmount)
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var subAgents = await context.Agents.Where(x => x.ParentAgentId == mainAgent.Id)
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

        var agentDividends = await context.AgentDividends
            .Include(x => x.AgentDividendMultipleLayers)
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.From >= request.From.ToUniversalTime())
            .Where(x => x.To < request.To.ToUniversalTime())
            .Where(x => agentIds.Contains(x.AgentId))
            .GroupJoin(context.AgentDividendPlatformCharges,
                a => a.Id,
                a => a.AgentDividendId,
                (agentDividend, platformCharges) => new { agentDividend, platformCharges })
            .GroupJoin(context.AgentDividendProductFees,
                a => a.agentDividend.Id,
                a => a.AgentDividendId,
                (group, productFees) => new { group.agentDividend, group.platformCharges, productFees })
            .GroupBy(x => new { x.agentDividend.AgentId, x.agentDividend.AgentDividendPercentage },
                (x, list) => new
                {
                    x.AgentId,
                    x.AgentDividendPercentage,
                    CompanyAdjustAmount = list.Sum(l => l.agentDividend.CompanyAdjustAmount),
                    AgentAdjustAmount = list.Sum(l => l.agentDividend.AgentAdjustAmount),
                    BonusAdjustAmount = list.Sum(l => l.agentDividend.BonusAdjustAmount),
                    BonusAmount = list.Sum(l => l.agentDividend.BonusAmount),
                    BetAmount = list.Sum(l => l.agentDividend.BetAmount),
                    OriginalTurnover = list.Sum(l => l.agentDividend.OriginalTurnover),
                    WinLossAmount = list.Sum(l => l.agentDividend.WinLossAmount),
                    NetProfit = list.Sum(l => l.agentDividend.NetProfit),
                    AgentDividendAmount =
                        list.Sum(l => l.agentDividend.BeforeDividendAmount ?? l.agentDividend.AgentDividendAmount),
                    NoIncludeNetProfitAgentAdjustAmount =
                        list.Sum(l => l.agentDividend.NoIncludeNetProfitAgentAdjustAmount),
                    NoIncludeNetProfitBonusAdjustAmount =
                        list.Sum(l => l.agentDividend.NoIncludeNetProfitBonusAdjustAmount),
                    FromPlayerAgentDividendAmount =
                        list.Sum(l => l.agentDividend.AgentDividendMultipleLayers
                            .Where(m => m.AgentId == m.DividendProviderAgentId)
                            .Sum(s => s.Dividend)),
                    FromTeamAgentDividendAmount = list.Sum(l
                        => l.agentDividend.AgentDividendMultipleLayers
                            .Where(m => m.AgentId != m.DividendProviderAgentId)
                            .Sum(s => s.Dividend)),
                    PlatformCharge = list.Sum(l => l.platformCharges.Sum(s => s.WinLossAmount * s.Percentage / 100)),
                    ProductFee = list.Sum(l => l.productFees.Sum(s => s.WinLossAmount * s.Percentage / 100))
                })
            .ToArrayAsync(cancellationToken);

        var agentCovered = await context.AgentDividendMultipleLayers
            .Include(x => x.AgentDividend)
            .Where(x => x.AgentDividend.BrandId == request.BrandId)
            .Where(x => x.AgentDividend.From >= request.From.ToUniversalTime())
            .Where(x => x.AgentDividend.To < request.To.ToUniversalTime())
            .Where(x => agentIds.Contains(x.AgentId))
            .Where(x => x.AgentId == x.DividendProviderAgentId)
            .Where(x => x.AgentCoveredBonus != null || x.AgentCoveredBonusAdjust != null)
            .GroupBy(x => x.AgentId, (agentId, list) => new
            {
                AgentId = agentId,
                AgentCoveredBonus = list.Sum(l => l.AgentCoveredBonus),
                AgentCoveredBonusAdjust = list.Sum(l => l.AgentCoveredBonusAdjust)
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var models = agentDividends.Join(agents,
                ad => ad.AgentId,
                a => a.Id,
                (ad, a) => new
                {
                    Id = ad.AgentId,
                    Username = a.Username,
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
                    AgentDividendPercentage = ad.AgentDividendPercentage,
                    PlatformCharge = ad.PlatformCharge,
                    ProductFee = ad.ProductFee
                })
            .GroupJoin(agentCovered,
                x => x.Id,
                c => c.AgentId,
                (ad, list) => new { ad, list })
            .SelectMany(x => x.list.DefaultIfEmpty(), (x, covered) => new { x.ad, covered })
            .Select(x => new AgentDividendModel
            {
                Id = x.ad.Id,
                Username = x.ad.Username,
                DepositAmount = dailyDepositWithdraws.FirstOrDefault(a => a.AgentId == x.ad.Id)?.DailyDepositAmount ??
                                0M,
                WithdrawalAmount =
                    dailyDepositWithdraws.FirstOrDefault(a => a.AgentId == x.ad.Id)?.DailyWithdrawAmount ?? 0M,
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
                AgentCoveredBonusAdjust = x.covered?.AgentCoveredBonusAdjust,
                PlatformCharge = x.ad.PlatformCharge,
                ProductFee = x.ad.ProductFee
            })
            .ToArray();

        return models.AsQueryable().CreatePagedResponse(request);
    }
}