using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent
{
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
            var mainAgentDividends = await _agentDividendService.GetQueries()
                .Where(x => x.AgentId == request.AgentId)
                .Where(x => x.From >= request.From.ToUniversalTime())
                .Where(x => x.To < request.To.ToUniversalTime())
                .Include(x => x.AgentDividendMultipleLayers)
                .Select(x => new
                {
                    x.AgentId,
                    x.From,
                    x.To,
                    x.AgentDividendPercentage,
                    AgentDividendMultipleLayers = x.AgentDividendMultipleLayers.Select(a => new
                    {
                        a.AgentId,
                        a.Dividend,
                        a.DividendProviderAgentId,
                        a.Layer
                    })
                })
                .ToArrayAsync(cancellationToken).ConfigureAwait(false);

            var dividendProviderAgents = mainAgentDividends.SelectMany(x => x.AgentDividendMultipleLayers)
                .Where(x => x.AgentId == request.AgentId)
                .Select(x => new
                {
                    x.DividendProviderAgentId,
                    x.Dividend,
                    GroupAgentId = x.Layer.Split(',').Where((s, i) => i == 1).FirstOrDefault()?.ToString()
                }).Distinct().ToArray();

            var firstLayerAgentDividendIds = dividendProviderAgents.Where(x => x.GroupAgentId != null)
                .Select(x => x.DividendProviderAgentId).ToArray();

            var firstLayerGroupAgentIds = dividendProviderAgents.Where(x => x.GroupAgentId != null)
                .Select(x => x.GroupAgentId == null ? (Guid?)null : Guid.Parse(x.GroupAgentId)).ToArray();

            var fistLayerAgentDividends = await _agentDividendService.GetQueries()
                .Where(x => firstLayerGroupAgentIds.Contains(x.AgentId))
                .Where(x => x.From >= request.From.ToUniversalTime())
                .Where(x => x.To < request.To.ToUniversalTime())
                .Select(x => new
                {
                    x.AgentId,
                    x.From,
                    x.To,
                    x.AgentDividendPercentage
                })
                .ToArrayAsync(cancellationToken).ConfigureAwait(false);

            var allAgentDividends = await _agentDividendService.GetQueries()
                .Where(x => firstLayerAgentDividendIds.Contains(x.AgentId))
                .Where(x => x.From >= request.From.ToUniversalTime())
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
                    x.AgentAdjustAmount,
                    x.CreatedOn
                })
                .Where(x => x.To < request.To.ToUniversalTime()).ToArrayAsync(cancellationToken).ConfigureAwait(false);

            var agentDividends = allAgentDividends
                .Join(dividendProviderAgents.Where(x => x.GroupAgentId != null), ad => ad.AgentId,
                    da => da.DividendProviderAgentId, (agentDividend, group)
                        => new
                        {
                            agentDividend.Id,
                            agentDividend.AgentId,
                            group.DividendProviderAgentId,
                            agentDividend.From,
                            agentDividend.To,
                            agentDividend.NetProfit,
                            agentDividend.LiveDealerWinLoss,
                            SlotWinLoss =
                                agentDividend.SlotWinLoss + agentDividend.FishingWinLoss + agentDividend.ArcadeWinLoss +
                                agentDividend.TableGameWinLoss,
                            agentDividend.LotteryWinLoss,
                            agentDividend.CardGameWinLoss,
                            SportsWinLoss = agentDividend.SportsWinLoss + agentDividend.ESportsWinLoss,
                            agentDividend.VirtualSportsWinLoss,
                            agentDividend.WinLossAmount,
                            ExpenditureAmount =
                                agentDividend.PromotionAmount + agentDividend.RebateAmount +
                                agentDividend.AgentAdjustAmount,
                            BonusAmount = agentDividend.RebateAmount + agentDividend.PromotionAmount,
                            agentDividend.AgentAdjustAmount,
                            agentDividend.CreatedOn,
                            group.GroupAgentId,
                            AgentDividendPercentage = group.GroupAgentId == null
                                ? mainAgentDividends
                                    .First(m => m.From == agentDividend.From && m.To == agentDividend.To)
                                    .AgentDividendPercentage
                                : mainAgentDividends
                                    .First(m => m.From == agentDividend.From && m.To == agentDividend.To)
                                    .AgentDividendPercentage -
                                fistLayerAgentDividends
                                    .First(m => m.AgentId.ToString() == group.GroupAgentId &&
                                        m.From == agentDividend.From && m.To == agentDividend.To)
                                    .AgentDividendPercentage,
                            AgentDividendAmount = group.Dividend
                        })
                .Where(x => x.AgentDividendAmount > 0)
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
                    Username = a.Username,
                    SettlementDate = ad.CreatedOn,
                    SettlementDateFrom = ad.From,
                    SettlementDateTo = ad.To,
                    LiveDealerWinLoss = ad.LiveDealerWinLoss,
                    SlotWinLoss = ad.SlotWinLoss,
                    LotteryWinLoss = ad.LotteryWinLoss,
                    CardGameWinLoss = ad.CardGameWinLoss,
                    SportsWinLoss = ad.SportsWinLoss,
                    VirtualSportsWinLoss = ad.VirtualSportsWinLoss,
                    WinLossAmount = ad.WinLossAmount,
                    AgentDividendAmount = ad.AgentDividendAmount,
                    AgentDividendPercentage = ad.AgentDividendPercentage,
                    ExpenditureAmount = ad.ExpenditureAmount,
                    BonusAmount = ad.BonusAmount,
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
                },(x,list)=> new AgentDividendTeamDetailModel
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
                    CompanyWinLossTotalAmount = x.LiveDealerWinLoss + x.SlotWinLoss + x.LotteryWinLoss + x.CardGameWinLoss + x.SportsWinLoss + x.VirtualSportsWinLoss
                }).ToArray();

            return models.AsQueryable().CreatePagedResponse(request);
        }
    }
}