using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent
{
    public class
        GetAgentContributionsQueryHandler : IRequestHandler<GetAgentContributionsRequest,
            PagedResponse<AgentContributionModel>>
    {
        private readonly ITgsService<Repository.Entities.Agent> _agentService;
        private readonly ITgsService<WithdrawForm> _withdrawFormsService;
        private readonly ITgsService<Repository.Entities.Player> _playerService;
        private readonly ITgsService<PlayerAggregate> _playerAggregateService;

        public GetAgentContributionsQueryHandler(
            ITgsService<Repository.Entities.Agent> agentService,
            ITgsService<WithdrawForm> withdrawFormsService,
            ITgsService<Repository.Entities.Player> playerService,
            ITgsService<PlayerAggregate> playerAggregateService)
        {
            _agentService = agentService;
            _withdrawFormsService = withdrawFormsService;
            _playerService = playerService;
            _playerAggregateService = playerAggregateService;
        }

        public async Task<PagedResponse<AgentContributionModel>> Handle(
            GetAgentContributionsRequest request,
            CancellationToken cancellationToken)
        {
            var agent = await _agentService.GetQueries().Include(x => x.Players)
                .Where(x => x.BrandId == request.BrandId)
                .Where(request.AgentId, a => a.Id == request.AgentId)
                .Where(request.Username, a => a.Username == request.Username).FirstOrDefaultAsync(cancellationToken);

            if (agent == null)
            {
                return new PagedResponse<AgentContributionModel>
                {
                    Valid = false,
                    Message = Resources.Agent.NotFound
                };
            }

            var playerWithdrawals = await _withdrawFormsService.GetQueries()
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.UserType == 1 && x.Status == 9000)
                .Where(x => x.CreatedOn >= request.From.ToUniversalTime() && x.CreatedOn < request.To.ToUniversalTime())
                .Join(_playerService.GetQueries(),
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

            var players = agent.Players.GroupJoin(_playerAggregateService.GetQueries()
                        .Where(x => x.BrandId == request.BrandId)
                        .Where(x => x.AgentId == agent.Id)
                        .Where(x => x.ReportDate >= request.From.ToUniversalTime() &&
                            x.ReportDate < request.To.ToUniversalTime()),
                    p => p.PlayerCode,
                    playerAggregate => playerAggregate.PlayerCode,
                    (player, players) => new
                    {
                        player,
                        players
                    })
                .SelectMany(x => x.players.DefaultIfEmpty(),
                    (x, playerAggregate) => new
                    {
                        x.player,
                        playerAggregate
                    })
                .GroupBy(x => new
                    {
                        x.player.Username,
                        x.player.PlayerCode
                    },
                    (player, playerAggregate) =>
                        new AgentContributionModel
                        {
                            Username = player.Username,
                            Type = AccountType.Player,
                            DepositAmount = playerAggregate.Sum(p => p.playerAggregate?.DepositAmount ?? 0),
                            WithdrawalAmount =
                                playerWithdrawals.FirstOrDefault(w => w.PlayerCode == player.PlayerCode)
                                    ?.WithdrawalAmount ?? 0,
                            CompanyAdjustAmount = playerAggregate.Sum(p => p.playerAggregate == null
                                ? 0
                                : p.playerAggregate.CompanyAdjustAmount -
                                p.playerAggregate.NoIncludeNetProfitCompanyAdjustAmount),
                            AgentAdjustAmount = playerAggregate.Sum(p
                                => p.playerAggregate == null
                                    ? 0
                                    : p.playerAggregate.AgentAdjustAmount -
                                    p.playerAggregate.NoIncludeNetProfitAgentAdjustAmount),
                            BonusAmount =
                                playerAggregate.Sum(p
                                    => p.playerAggregate == null
                                        ? 0
                                        : p.playerAggregate.RebateAmount + p.playerAggregate.PromotionAmount),
                            BetAmount = playerAggregate.Sum(p => p.playerAggregate?.BetAmount ?? 0),
                            ValidBetAmount = playerAggregate.Sum(p => p.playerAggregate?.Turnover ?? 0),
                            Turnover = playerAggregate.Sum(p => p.playerAggregate?.Turnover ?? 0),
                            WinLossAmount = playerAggregate.Sum(p => p.playerAggregate?.WinLossAmount ?? 0),
                            NetProfit = playerAggregate.Sum(p => p.playerAggregate == null
                                ? 0
                                : p.playerAggregate.WinLossAmount - (p.playerAggregate.AgentAdjustAmount -
                                    p.playerAggregate.NoIncludeNetProfitAgentAdjustAmount) -
                                p.playerAggregate.RebateAmount -
                                p.playerAggregate.PromotionAmount)
                        }).ToArray();

            if (!request.IncludeAgents)
            {
                return await players
                    .CreatePagedResponseAsync(players.Length);
            }

            var subAgents = await _agentService.GetQueries().Where(x => x.ParentAgentId == agent.Id)
                .Include(x => x.Players)
                .Select(x => new
                {
                    x.Id,
                    x.Username,
                    SubAgents = x.InverseParentAgent.Select(i => new
                    {
                        i.Id,
                        i.Username
                    })
                })
                .ToArrayAsync(cancellationToken);

            var allSubAgents = subAgents.Select(x => new
            {
                MainAgentId = x.Id,
                AgentId = x.Id
            }).Union(subAgents.SelectMany(x => x.SubAgents.Select(s => new
            {
                MainAgentId = x.Id,
                AgentId = s.Id
            }))).ToArray();

            var allSubAgentIds = allSubAgents.Select(x => x.AgentId).ToArray();

            var childAgentPlayers = subAgents.GroupJoin(allSubAgents.Join(_playerAggregateService.GetQueries()
                            .Where(x => x.BrandId == request.BrandId)
                            .Where(x => x.AgentId != null
                                && allSubAgentIds.Contains(x.AgentId.Value))
                            .Where(x => x.ReportDate >= request.From.ToUniversalTime() &&
                                x.ReportDate < request.To.ToUniversalTime()), agent => agent.AgentId,
                        agg => agg.AgentId,
                        (agent, aggregate) => new
                        {
                            agent,
                            aggregate
                        })
                    ,
                    sub => sub.Id,
                    playerAggregate => playerAggregate.agent.MainAgentId,
                    (agent, players) => new
                    {
                        agent,
                        players
                    })
                .SelectMany(x => x.players.DefaultIfEmpty(),
                    (x, playerAggregate) => new
                    {
                        x.agent,
                        playerAggregate = playerAggregate?.aggregate,
                        SubAgentId = playerAggregate?.agent.AgentId
                    })
                .GroupBy(x => new
                    {
                        x.agent.Username,
                        x.agent.Id
                    },
                    (agent, playerAggregate) =>
                        new AgentContributionModel
                        {
                            Username = agent.Username,
                            Type = AccountType.Agent,
                            DepositAmount = playerAggregate.Sum(p => p.playerAggregate?.DepositAmount ?? 0),
                            WithdrawalAmount =
                                playerWithdrawals
                                    .Where(w => playerAggregate.Select(p => p.SubAgentId).Contains(w.AgentId))
                                    .Sum(w => w.WithdrawalAmount),
                            CompanyAdjustAmount = playerAggregate.Sum(p => p.playerAggregate?.CompanyAdjustAmount ?? 0),
                            AgentAdjustAmount = playerAggregate.Sum(p => p.playerAggregate?.AgentAdjustAmount ?? 0),
                            BonusAmount =
                                playerAggregate.Sum(p
                                    => p.playerAggregate == null
                                        ? 0
                                        : p.playerAggregate.RebateAmount + p.playerAggregate.PromotionAmount),
                            BetAmount = playerAggregate.Sum(p => p.playerAggregate?.BetAmount ?? 0),
                            ValidBetAmount = playerAggregate.Sum(p => p.playerAggregate?.OriginalTurnover ?? 0),
                            Turnover = playerAggregate.Sum(p => p.playerAggregate?.Turnover ?? 0),
                            WinLossAmount = playerAggregate.Sum(p => p.playerAggregate?.WinLossAmount ?? 0),
                            NetProfit = playerAggregate.Sum(p => p.playerAggregate == null
                                ? 0
                                : p.playerAggregate.WinLossAmount - p.playerAggregate.AgentAdjustAmount -
                                p.playerAggregate.RebateAmount -
                                p.playerAggregate.PromotionAmount)
                        }).ToArray();


            return childAgentPlayers.Union(players).AsQueryable()
                .CreatePagedResponse(request);
        }
    }
}