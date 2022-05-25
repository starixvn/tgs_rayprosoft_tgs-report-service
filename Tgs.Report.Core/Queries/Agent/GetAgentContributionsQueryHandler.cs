using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent;

public class
    GetAgentContributionsQueryHandler : IRequestHandler<GetAgentContributionsRequest,
        PagedResponse<AgentContributionModel>>
{
    private readonly ITgsService<Repository.Entities.Agent> _agentService;
    private readonly ITgsService<PlayerAggregate> _playerAggregateService;
    private readonly ITgsService<Repository.Entities.Player> _playerService;
    private readonly ITgsService<WithdrawForm> _withdrawFormsService;

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
            return new()
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
                        BonusAdjustAmount = playerAggregate.Sum(p 
                            => p.playerAggregate == null
                                ? 0
                                : p.playerAggregate.BonusAdjustAmount -
                                p.playerAggregate.NoIncludeNetProfitBonusAdjustAmount),
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
            return players.AsQueryable()
                .CreatePagedResponse(request);
        }

        var nLevel = await _agentService.GetQueries().Where(x => x.Id == agent.Id).Select(x => x.Path.NLevel)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        var firstLayerSubAgents = await _agentService.GetQueries().Where(x => x.ParentAgentId == agent.Id)
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

        var allSubAgents = (await _agentService.GetQueries().Where(x => x.Path.IsDescendantOf(new(agent.Path)))
                .Where(x => x.Id != agent.Id)
                .Select(x => new
                {
                    AgentId = x.Id,
                    x.Path.NLevel,
                    MainAgentIdString = x.Path.Subtree(nLevel, nLevel+1)
                }).ToArrayAsync(cancellationToken).ConfigureAwait(false))
            .Select(x => new
            {
                x.AgentId,
                MainAgentId = Guid.Parse(x.MainAgentIdString)
            }).ToArray();

        var allSubAgentIds = allSubAgents.Select(x => x.AgentId).ToArray();

        var childAgentPlayers = firstLayerSubAgents.GroupJoin(allSubAgents.Join(_playerAggregateService.GetQueries()
                        .Where(x => x.BrandId == request.BrandId)
                        .Where(x => x.ReportDate >= request.From.ToUniversalTime() &&
                            x.ReportDate < request.To.ToUniversalTime()), agent => agent.AgentId,
                    agg => agg.AgentId,
                    (groupAgent, aggregate) => new
                    {
                        groupAgent,
                        aggregate
                    })
                ,
                sub => sub.Id,
                playerAggregate => playerAggregate.groupAgent.MainAgentId,
                (groupAgent, GroupPlayers) => new
                {
                    groupAgent,
                    GroupPlayers
                })
            .SelectMany(x => x.GroupPlayers.DefaultIfEmpty(),
                (x, playerAggregate) => new
                {
                    x.groupAgent,
                    playerAggregate = playerAggregate?.aggregate,
                    SubAgentId = playerAggregate?.groupAgent.AgentId
                })
            .GroupBy(x => new
                {
                    x.groupAgent.Username,
                    x.groupAgent.Id
                },
                (groupAgent, playerAggregate) =>
                    new AgentContributionModel
                    {
                        Username = groupAgent.Username,
                        Type = AccountType.Agent,
                        DepositAmount = playerAggregate.Sum(p => p.playerAggregate?.DepositAmount ?? 0),
                        WithdrawalAmount =
                            playerWithdrawals
                                .Where(x => x.AgentId != null)
                                .Where(w => playerAggregate.Select(p => p.SubAgentId).Contains(w.AgentId)
                                    || w.AgentId != null && playerAggregate.Select(p => p.groupAgent.Id)
                                        .Contains(w.AgentId.Value))
                                .Sum(w => w.WithdrawalAmount),
                        CompanyAdjustAmount = playerAggregate.Sum(p => p.playerAggregate == null
                            ? 0 : p.playerAggregate.CompanyAdjustAmount - p.playerAggregate.NoIncludeNetProfitCompanyAdjustAmount),
                        AgentAdjustAmount = playerAggregate.Sum(p => p.playerAggregate == null
                            ? 0 : p.playerAggregate.AgentAdjustAmount - p.playerAggregate.NoIncludeNetProfitAgentAdjustAmount),
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
                            : p.playerAggregate.WinLossAmount - (p.playerAggregate.AgentAdjustAmount -
                                p.playerAggregate.NoIncludeNetProfitAgentAdjustAmount) -
                            p.playerAggregate.RebateAmount -
                            p.playerAggregate.PromotionAmount)
                    }).ToArray();

        return childAgentPlayers.Union(players).AsQueryable()
            .CreatePagedResponse(request);
    }
}