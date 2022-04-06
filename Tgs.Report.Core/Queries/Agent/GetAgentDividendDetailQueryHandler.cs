using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent
{
    public class
        GetAgentDividendDetailQueryHandler : IRequestHandler<GetAgentDividendDetailsRequest,
            PagedResponse<AgentDividendDetailModel>>
    {
        private readonly ITgsRepository _repository;

        public GetAgentDividendDetailQueryHandler(ITgsRepository repository) => _repository = repository;

        public async Task<PagedResponse<AgentDividendDetailModel>> Handle(
            GetAgentDividendDetailsRequest request,
            CancellationToken cancellationToken)
        {
            var agentDividends = await _repository.AgentDividends.Where(x => x.AgentId == request.AgentId)
                .Where(x => x.From >= request.From.ToUniversalTime())
                .Where(x => x.To < request.To.ToUniversalTime())
                .GroupBy(x => new
                    {
                        x.AgentId,
                        x.From,
                        x.To,
                        x.CreatedOn,
                        x.AgentDividendPercentage
                    },
                    (x, list) => new
                    {
                        x.AgentId,
                        x.From,
                        x.To,
                        x.CreatedOn,
                        x.AgentDividendPercentage,
                        LiveDealerWinLoss = list.Sum(l => l.LiveDealerWinLoss),
                        SlotWinLoss =
                            list.Sum(l => l.SlotWinLoss + l.FishingWinLoss + l.ArcadeWinLoss + l.TableGameWinLoss),
                        LotteryWinLoss = list.Sum(l => l.LotteryWinLoss),
                        CardGameWinLoss = list.Sum(l => l.CardGameWinLoss),
                        SportsWinLoss = list.Sum(l => l.SportsWinLoss + l.ESportsWinLoss),
                        VirtualSportsWinLoss = list.Sum(l => l.VirtualSportsWinLoss),
                        WinLossAmount = list.Sum(l => l.WinLossAmount),
                        AgentDividendAmount =
                            list.Where(l => l.AgentDividendAmount > 0).Sum(l => l.AgentDividendAmount),
                        ExpenditureAmount = list.Sum(l => l.PromotionAmount + l.RebateAmount + l.AgentAdjustAmount)
                    }).ToArrayAsync(cancellationToken);

            var agentIds = agentDividends.Select(x => x.AgentId).ToArray();
            var agents = await _repository.Agents.Where(x => agentIds.Contains(x.Id))
                .ToArrayAsync(cancellationToken);

            var models = agentDividends.Join(agents, ad => ad.AgentId, a => a.Id,
                (ad, a) => new AgentDividendDetailModel
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
                    ExpenditureAmount = ad.ExpenditureAmount
                }).ToArray();

            return models.AsQueryable().CreatePagedResponse(request);
        }
    }
}