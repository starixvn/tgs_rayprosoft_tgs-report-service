using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent
{
    public class
        GetAgentDividendQueryHandler : IRequestHandler<GetAgentDividendsRequest, PagedResponse<AgentDividendModel>>
    {
        private readonly ITgsRepository _repository;

        public GetAgentDividendQueryHandler(ITgsRepository repository) => _repository = repository;

        public async Task<PagedResponse<AgentDividendModel>> Handle(
            GetAgentDividendsRequest request,
            CancellationToken cancellationToken)
        {
            var mainAgent = await _repository.Agents.Where(x => x.BrandId == request.BrandId)
                .Where(request.Username, x => request.Username == x.Username)
                .Where(request.AgentId, x => x.Id == request.AgentId).FirstOrDefaultAsync(cancellationToken);

            if (mainAgent is null)
            {
                return new PagedResponse<AgentDividendModel>
                {
                    Valid = false,
                    Message = Resources.Agent.NotFound
                };
            }

            var playerWithdrawals = await _repository.WithdrawForms
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.UserType == 1 && x.Status == 9000)
                .Where(x => x.CreatedOn >= request.From.ToUniversalTime() && x.CreatedOn < request.To.ToUniversalTime())
                .Join(_repository.Players,
                    withdrawForm => withdrawForm.UserId,
                    player => player.PlayerCode,
                    (withdrawForm, player) => new
                    {
                        player.AgentId,
                        player.PlayerCode,
                        WithdrawalAmount = withdrawForm.ActualAmount ?? withdrawForm.Amount
                    })
                .GroupBy(x => new
                    {
                        x.AgentId,
                        x.PlayerCode
                    },
                    (x, withdrawals) => new
                    {
                        x.AgentId,
                        x.PlayerCode,
                        WithdrawalAmount = withdrawals.Sum(w => w.WithdrawalAmount)
                    })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            var subAgents = await _repository.Agents.Where(x => x.ParentAgentId == mainAgent.Id)
                .ToListAsync(cancellationToken);

            var agents = new List<Repository.Entities.Agent> { mainAgent };
            agents.AddRange(subAgents);

            var agentIds = agents.Select(x => x.Id).ToArray();

            var agentDividends = await _repository.AgentDividends
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
                    x.NoIncludeNetProfitBonusAdjustAmount
                }).ToArrayAsync(cancellationToken);

            var agentDividendGroup = agentDividends.GroupBy(x => x.AgentId,
                (agentId, list) => new
                {
                    AgentId = agentId,
                    DepositAmount = list.Sum(l => l.DepositAmount),
                    WithdrawalAmount = list.Sum(l => l.WithdrawalAmount),
                    CompanyAdjustAmount = list.Sum(l => l.CompanyAdjustAmount),
                    AgentAdjustAmount = list.Sum(l => l.AgentAdjustAmount),
                    BonusAdjustAmount = list.Sum(l=> l.BonusAdjustAmount),
                    BonusAmount = list.Sum(l => l.BonusAmount),
                    BetAmount = list.Sum(l => l.BetAmount),
                    ValidBetAmount = list.Sum(l => l.ValidBetAmount),
                    OriginalTurnover = list.Sum(l => l.OriginalTurnover),
                    Turnover = list.Sum(l => l.Turnover),
                    WinLossAmount = list.Sum(l => l.WinLossAmount),
                    NetProfit = list.Sum(l => l.NetProfit),
                    AgentDividendAmount =
                        list.Sum(l => l.BeforeDividendAmount ?? l.AgentDividendAmount),
                    NoIncludeNetProfitAgentAdjustAmount = list.Sum(l => l.NoIncludeNetProfitAgentAdjustAmount),
                    NoIncludeNetProfitBonusAdjustAmount = list.Sum(l => l.NoIncludeNetProfitBonusAdjustAmount),
                    FromPlayerAgentDividendAmount = list.Sum(l => l.AgentDividendMultipleLayers.Where(m => m.AgentId == m.DividendProviderAgentId).Sum(s => s.Dividend)),
                    FromTeamAgentDividendAmount = list.Sum(l => l.AgentDividendMultipleLayers.Where(m => m.AgentId != m.DividendProviderAgentId).Sum(s => s.Dividend)),
                }).ToArray();

            var models = agentDividendGroup.Join(agents.ToArray(),
                ad => ad.AgentId,
                a => a.Id,
                (ad, a) => new AgentDividendModel
                {
                    Id = ad.AgentId,
                    Username = a.Username,
                    DepositAmount = ad.DepositAmount,
                    WithdrawalAmount = playerWithdrawals.Where(w => w.AgentId == ad.AgentId)
                        .Sum(w => w.WithdrawalAmount),
                    CompanyAdjustAmount = ad.CompanyAdjustAmount,
                    AgentAdjustAmount = ad.AgentAdjustAmount - ad.NoIncludeNetProfitAgentAdjustAmount,
                    BonusAdjustAmount = ad.BonusAdjustAmount - ad.NoIncludeNetProfitBonusAdjustAmount,
                    BonusAmount = ad.BonusAmount,
                    BetAmount = ad.BetAmount,
                    ValidBetAmount = ad.OriginalTurnover,
                    Turnover = ad.Turnover,
                    WinLossAmount = ad.WinLossAmount,
                    NetProfit = ad.NetProfit,
                    AgentDividendAmount = ad.AgentDividendAmount,
                    FromPlayerAgentDividendAmount = ad.FromPlayerAgentDividendAmount,
                    FromTeamAgentDividendAmount = ad.FromTeamAgentDividendAmount,
                }).ToArray();

            return models.AsQueryable().CreatePagedResponse(request);
        }
    }
}