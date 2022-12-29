using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent;

public class
    GetAgentDividendTeamDetailQueryHandler : IRequestHandler<GetAgentDividendTeamDetailsRequest,
        PagedResponse<AgentDividendTeamDetailModel>>
{
    private readonly ITgsService<AgentDividend> _agentDividendService;
    private readonly ITgsService<Repository.Entities.Agent> _agentService;

    public GetAgentDividendTeamDetailQueryHandler(ITgsService<Repository.Entities.Agent> agentService,
        ITgsService<AgentDividend> agentDividendService)
    {
        _agentService = agentService;
        _agentDividendService = agentDividendService;
    }

    public async Task<PagedResponse<AgentDividendTeamDetailModel>> Handle(
        GetAgentDividendTeamDetailsRequest request,
        CancellationToken cancellationToken)
    {
        var dividendProviderAgents = await _agentDividendService.GetQueries()
            .Where(x => x.AgentId == request.AgentId)
            .Where(x => x.AgentDividendPercentage == request.AgentDividendPercentage)
            .Where(x => x.From >= request.From.ToUniversalTime())
            .Where(x => x.To < request.To.ToUniversalTime())
            .Include(x => x.AgentDividendMultipleLayers)
            .SelectMany(x => x.AgentDividendMultipleLayers.Select(s => new
            {
                x.AgentDividendPercentage,
                x.From,
                x.To,
                s.DividendProviderAgentId,
                s.Dividend,
                s.ActualDividendPercentage,
                s.AgentCoveredBonus,
                s.AgentCoveredBonusAdjust
            }))
            .Where(x => x.DividendProviderAgentId != request.AgentId)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);

        var firstLayerAgentDividendIds = dividendProviderAgents.Select(x => x.DividendProviderAgentId).ToArray();

        var allAgentDividends = await _agentDividendService.GetQueries()
            .Where(x => firstLayerAgentDividendIds.Contains(x.AgentId))
            .Where(x => x.From >= request.From.ToUniversalTime())
            .Where(x => x.To < request.To.ToUniversalTime())
            .Select(x => new
            {
                x.Id,
                x.AgentId,
                x.From,
                x.To,
                x.NetProfit,
                x.LiveDealerWinLoss,
                x.SlotWinLoss,
                x.FishingWinLoss,
                x.ArcadeWinLoss,
                x.TableGameWinLoss,
                x.LotteryWinLoss,
                x.CardGameWinLoss,
                x.SportsWinLoss,
                x.ESportsWinLoss,
                x.VirtualSportsWinLoss,
                x.WinLossAmount,
                x.PromotionAmount,
                x.RebateAmount,
                x.BonusAmount,
                x.BonusAdjustAmount,
                x.NoIncludeNetProfitBonusAdjustAmount,
                x.CreatedOn,
                x.AgentDividendPercentage
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var agentDividends = allAgentDividends
            .GroupJoin(dividendProviderAgents, ad => new
                {
                    ad.AgentId,
                    ad.From,
                    ad.To
                },
                da => new
                {
                    AgentId = da.DividendProviderAgentId,
                    da.From,
                    da.To
                }, (agentDividend, group)
                    => new
                    {
                        agentDividend,
                        group
                    })
            .SelectMany(x => x.group.DefaultIfEmpty(),
                (x, provider) => new
                {
                    x.agentDividend.Id,
                    x.agentDividend.AgentId,
                    provider?.DividendProviderAgentId,
                    x.agentDividend.From,
                    x.agentDividend.To,
                    x.agentDividend.NetProfit,
                    x.agentDividend.LiveDealerWinLoss,
                    SlotWinLoss =
                        x.agentDividend.SlotWinLoss + x.agentDividend.FishingWinLoss + x.agentDividend.ArcadeWinLoss +
                        x.agentDividend.TableGameWinLoss,
                    x.agentDividend.LotteryWinLoss,
                    x.agentDividend.CardGameWinLoss,
                    SportsWinLoss = x.agentDividend.SportsWinLoss + x.agentDividend.ESportsWinLoss,
                    x.agentDividend.VirtualSportsWinLoss,
                    x.agentDividend.WinLossAmount,
                    ExpenditureAmount =
                        x.agentDividend.BonusAmount +
                        (x.agentDividend.BonusAdjustAmount - x.agentDividend.NoIncludeNetProfitBonusAdjustAmount),
                    BonusAmount = x.agentDividend.RebateAmount + x.agentDividend.PromotionAmount,
                    x.agentDividend.CreatedOn,
                    AgentDividendPercentage = provider?.ActualDividendPercentage ?? 0M,
                    AgentDividendAmount = provider?.Dividend ?? 0
                })
            .ToArray();

        var agentIds = agentDividends.Select(x => x.AgentId).ToArray();

        var agents = await _agentService.GetQueries().Where(x => agentIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.Username
            })
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);

        var models = agentDividends.Join(agents,
                ad => ad.AgentId,
                a => a.Id,
                (ad, a) => new
                {
                    Id = ad.AgentId,
                    a.Username,
                    SettlementDate = ad.CreatedOn,
                    SettlementDateFrom = ad.From,
                    SettlementDateTo = ad.To,
                    ad.LiveDealerWinLoss,
                    ad.SlotWinLoss,
                    ad.LotteryWinLoss,
                    ad.CardGameWinLoss,
                    ad.SportsWinLoss,
                    ad.VirtualSportsWinLoss,
                    ad.WinLossAmount,
                    ad.AgentDividendAmount,
                    ad.AgentDividendPercentage,
                    ad.ExpenditureAmount,
                    ad.BonusAmount,
                    ad.NetProfit
                })
            .GroupBy(x => new
            {
                x.Id,
                x.Username,
                x.SettlementDate,
                x.SettlementDateFrom,
                x.SettlementDateTo,
                x.LiveDealerWinLoss,
                x.SlotWinLoss,
                x.LotteryWinLoss,
                x.CardGameWinLoss,
                x.SportsWinLoss,
                x.VirtualSportsWinLoss,
                x.WinLossAmount,
                x.ExpenditureAmount,
                x.AgentDividendPercentage,
                x.BonusAmount,
                x.NetProfit
            }, (x, list) => new AgentDividendTeamDetailModel
            {
                Id = x.Id,
                Username = x.Username,
                SettlementDate = x.SettlementDate,
                SettlementDateFrom = x.SettlementDateFrom,
                SettlementDateTo = x.SettlementDateTo,
                LiveDealerWinLoss = x.LiveDealerWinLoss,
                SlotWinLoss = x.SlotWinLoss,
                LotteryWinLoss = x.LotteryWinLoss,
                CardGameWinLoss = x.CardGameWinLoss,
                SportsWinLoss = x.SportsWinLoss,
                VirtualSportsWinLoss = x.VirtualSportsWinLoss,
                WinLossAmount = x.WinLossAmount,
                AgentDividendAmount = list.Sum(l => l.AgentDividendAmount),
                AgentDividendPercentage = x.AgentDividendPercentage,
                ExpenditureAmount = x.ExpenditureAmount,
                BonusAmount = x.BonusAmount,
                NetProfit = x.NetProfit,
                CompanyWinLossTotalAmount = x.LiveDealerWinLoss + x.SlotWinLoss + x.LotteryWinLoss + x.CardGameWinLoss +
                    x.SportsWinLoss + x.VirtualSportsWinLoss
            }).ToArray();

        return models.AsQueryable().CreatePagedResponse(request);
    }
}