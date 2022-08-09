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
    private readonly ITgsService<PlayerBonusAggregate> _playerBonusAggregate;
    private readonly ITgsService<Repository.Entities.Player> _playerService;
    private readonly ITgsService<WithdrawForm> _withdrawFormsService;

    public GetAgentContributionsQueryHandler(
        ITgsService<Repository.Entities.Agent> agentService,
        ITgsService<WithdrawForm> withdrawFormsService,
        ITgsService<Repository.Entities.Player> playerService,
        ITgsService<PlayerAggregate> playerAggregateService,
        ITgsService<PlayerBonusAggregate> playerBonusAggregate)
    {
        _agentService = agentService;
        _withdrawFormsService = withdrawFormsService;
        _playerService = playerService;
        _playerAggregateService = playerAggregateService;
        _playerBonusAggregate = playerBonusAggregate;
    }

    public async Task<PagedResponse<AgentContributionModel>> Handle(
        GetAgentContributionsRequest request,
        CancellationToken cancellationToken)
    {
        var agent = await _agentService.GetQueries().Include(x => x.Players)
            .Where(x => x.BrandId == request.BrandId)
            .Where(request.AgentId, a => a.Id == request.AgentId)
            .Where(request.Username, a => a.Username == request.Username)
            .Select(x => new
            {
                x.Id,
                x.Path
            })
            .FirstOrDefaultAsync(cancellationToken);

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

        var playersPaged = await _playerAggregateService.GetQueries()
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.AgentId == agent.Id)
            .Where(x => x.ReportDate >= request.From.ToUniversalTime() &&
                x.ReportDate < request.To.ToUniversalTime())
            .Where(x => x.CreatedOn >= request.From.ToUniversalTime())
            .Select(x => x.PlayerCode).Union(_playerBonusAggregate.GetQueries()
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.AgentId == agent.Id)
                .Where(x => x.ReportDate >= request.From.ToUniversalTime() &&
                    x.ReportDate < request.To.ToUniversalTime())
                .Select(x => x.PlayerCode))
            .GroupBy(x => x, (x, y) => new { PlayerCode = x })
            .GroupJoin(_playerAggregateService.GetQueries()
                    .Where(x => x.BrandId == request.BrandId)
                    .Where(x => x.AgentId == agent.Id)
                    .Where(x => x.ReportDate >= request.From.ToUniversalTime() &&
                        x.ReportDate < request.To.ToUniversalTime())
                    .Where(x => x.CreatedOn >= request.From.ToUniversalTime())
                    .GroupBy(x => x.PlayerCode, (playerCode, list) => new
                    {
                        PlayerCode = playerCode,
                        DepositAmount = list.Sum(p => p.DepositAmount),
                        CompanyAdjustAmount = list.Sum(p => p.CompanyAdjustAmount -
                            p.NoIncludeNetProfitCompanyAdjustAmount),
                        AgentAdjustAmount = list.Sum(p
                            => p.AgentAdjustAmount -
                            p.NoIncludeNetProfitAgentAdjustAmount),
                        BonusAdjustAmount = list.Sum(p
                            => p.BonusAdjustAmount -
                            p.NoIncludeNetProfitBonusAdjustAmount),
                        BonusAmount =
                            list.Sum(p
                                => p.RebateAmount + p.PromotionAmount),
                        BetAmount = list.Sum(p => p.BetAmount),
                        ValidBetAmount = list.Sum(p => p.Turnover),
                        Turnover = list.Sum(p => p.Turnover),
                        WinLossAmount = list.Sum(p => p.WinLossAmount),
                        NetProfit = list.Sum(p => p.WinLossAmount
                            - (p.AgentAdjustAmount -
                                p.NoIncludeNetProfitAgentAdjustAmount)
                            - (p.BonusAdjustAmount -
                                p.NoIncludeNetProfitBonusAdjustAmount)
                            - p.RebateAmount
                            - p.PromotionAmount)
                    }),
                p => p.PlayerCode,
                playerAggregate => playerAggregate.PlayerCode,
                (player, players) => new
                {
                    player.PlayerCode,
                    players
                })
            .SelectMany(x => x.players.DefaultIfEmpty(),
                (x, playerAggregate) => new
                {
                    x.PlayerCode,
                    DepositAmount = playerAggregate == null ? 0M : playerAggregate.DepositAmount,
                    CompanyAdjustAmount = playerAggregate == null ? 0M : playerAggregate.CompanyAdjustAmount,
                    AgentAdjustAmount = playerAggregate == null ? 0M : playerAggregate.AgentAdjustAmount,
                    BonusAdjustAmount = playerAggregate == null ? 0M : playerAggregate.BonusAdjustAmount,
                    BonusAmount = playerAggregate == null ? 0M : playerAggregate.BonusAmount,
                    BetAmount = playerAggregate == null ? 0M : playerAggregate.BetAmount,
                    ValidBetAmount = playerAggregate == null ? 0M : playerAggregate.ValidBetAmount,
                    Turnover = playerAggregate == null ? 0M : playerAggregate.Turnover,
                    WinLossAmount = playerAggregate == null ? 0M : playerAggregate.WinLossAmount,
                    NetProfit = playerAggregate == null ? 0M : playerAggregate.NetProfit
                })
            .GroupJoin(_playerBonusAggregate.GetQueries()
                    .Where(x => x.BrandId == request.BrandId)
                    .Where(x => x.AgentId == agent.Id)
                    .Where(x => x.ReportDate >= request.From.ToUniversalTime() &&
                        x.ReportDate < request.To.ToUniversalTime())
                    .GroupBy(x => x.PlayerCode, (playerCode, list) => new
                    {
                        PlayerCode = playerCode,
                        UpgradeAmount = list.Sum(p => p.UpgradeAmount),
                        SeasonAmount = list.Sum(p => p.SeasonAmount),
                        BirthdayAmount = list.Sum(p
                            => p.BirthdayAmount)
                    })
                , player => player.PlayerCode, bonus => bonus.PlayerCode,
                (player, markets) => new
                {
                    player.PlayerCode,
                    player.DepositAmount,
                    player.CompanyAdjustAmount,
                    player.AgentAdjustAmount,
                    player.BonusAdjustAmount,
                    player.BonusAmount,
                    player.BetAmount,
                    player.ValidBetAmount,
                    player.Turnover,
                    player.WinLossAmount,
                    player.NetProfit,
                    markets
                })
            .SelectMany(x => x.markets.DefaultIfEmpty(),
                (x, bonus) => new
                {
                    x.PlayerCode,
                    x.DepositAmount,
                    x.CompanyAdjustAmount,
                    x.AgentAdjustAmount,
                    x.BonusAdjustAmount,
                    x.BonusAmount,
                    x.BetAmount,
                    x.ValidBetAmount,
                    x.Turnover,
                    x.WinLossAmount,
                    x.NetProfit,
                    UpgradeAmount = bonus == null ? 0M : bonus.UpgradeAmount,
                    SeasonAmount = bonus == null ? 0M : bonus.SeasonAmount,
                    BirthdayAmount = bonus == null ? 0M : bonus.BirthdayAmount
                })
            .Join(_playerService.GetQueries(), aggregate => aggregate.PlayerCode, player => player.PlayerCode,
                (agg, player) => new
                {
                    agg.PlayerCode,
                    player.Username,
                    agg.DepositAmount,
                    agg.CompanyAdjustAmount,
                    agg.AgentAdjustAmount,
                    agg.BonusAdjustAmount,
                    agg.BonusAmount,
                    agg.BetAmount,
                    agg.ValidBetAmount,
                    agg.Turnover,
                    agg.WinLossAmount,
                    agg.NetProfit,
                    agg.UpgradeAmount,
                    agg.SeasonAmount,
                    agg.BirthdayAmount
                })
            .GroupBy(x => new
                {
                    x.Username,
                    x.PlayerCode
                },
                (player, playerAggregate) =>
                    new
                    {
                        player.Username,
                        player.PlayerCode,
                        Type = AccountType.Player,
                        DepositAmount = playerAggregate.Sum(p => p.DepositAmount),
                        CompanyAdjustAmount = playerAggregate.Sum(p => p.CompanyAdjustAmount),
                        AgentAdjustAmount = playerAggregate.Sum(p
                            => p.AgentAdjustAmount),
                        BonusAdjustAmount = playerAggregate.Sum(p
                            => p.BonusAdjustAmount),
                        BonusAmount =
                            playerAggregate.Sum(p
                                => p.BonusAmount + p.UpgradeAmount + p.SeasonAmount + p.BirthdayAmount),
                        BetAmount = playerAggregate.Sum(p => p.BetAmount),
                        ValidBetAmount = playerAggregate.Sum(p => p.Turnover),
                        Turnover = playerAggregate.Sum(p => p.Turnover),
                        WinLossAmount = playerAggregate.Sum(p => p.WinLossAmount),
                        NetProfit = playerAggregate.Sum(p => p.NetProfit
                            - (p.UpgradeAmount + p.SeasonAmount + p.BirthdayAmount))
                    })
            .CreatePagedResponseAsync(request);

        var players = new PagedResponse<AgentContributionModel>
        {
            Valid = playersPaged.Valid,
            Message = playersPaged.Message,
            TotalCount = playersPaged.TotalCount,
            Items = playersPaged.Items?.Select(x => new AgentContributionModel
            {
                Username = x.Username,
                Type = x.Type,
                WithdrawalAmount =
                    playerWithdrawals.FirstOrDefault(w => w.PlayerCode == x.PlayerCode)
                        ?.WithdrawalAmount ?? 0,
                DepositAmount = x.DepositAmount,
                CompanyAdjustAmount = x.CompanyAdjustAmount,
                AgentAdjustAmount = x.AgentAdjustAmount,
                BonusAdjustAmount = x.BonusAdjustAmount,
                BonusAmount = x.BonusAmount,
                BetAmount = x.BetAmount,
                ValidBetAmount = x.ValidBetAmount,
                Turnover = x.Turnover,
                WinLossAmount = x.WinLossAmount,
                NetProfit = x.NetProfit
            }).ToArray()
        };

        if (!request.IncludeAgents)
        {
            return players;
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
                    MainAgentIdString = x.Path.Subtree(nLevel, nLevel + 1)
                }).ToArrayAsync(cancellationToken).ConfigureAwait(false))
            .Select(x => new
            {
                x.AgentId,
                MainAgentId = Guid.Parse(x.MainAgentIdString)
            }).ToArray();

        var agentPlayerAggregate = allSubAgents
            .GroupJoin(_playerAggregateService.GetQueries()
                    .Where(x => x.BrandId == request.BrandId)
                    .Where(x => x.ReportDate >= request.From.ToUniversalTime() &&
                        x.ReportDate < request.To.ToUniversalTime())
                    .Where(x => x.CreatedOn >= request.From.ToUniversalTime()),
                agent => agent.AgentId, playerAggregate => playerAggregate.AgentId,
                (agent, list) => new
                {
                    agent,
                    list
                })
            .SelectMany(x => x.list.DefaultIfEmpty(),
                (group, aggregate) => new
                {
                    group.agent.AgentId,
                    group.agent.MainAgentId,
                    aggregate
                })
            .GroupJoin(_playerBonusAggregate.GetQueries()
                    .Where(x => x.BrandId == request.BrandId)
                    .Where(x => x.ReportDate >= request.From.ToUniversalTime() &&
                        x.ReportDate < request.To.ToUniversalTime()), agent => agent.AgentId, bonus => bonus.AgentId,
                (agent, bonus) => new
                {
                    agent.AgentId,
                    agent.MainAgentId,
                    agent.aggregate,
                    bonus
                })
            .SelectMany(x => x.bonus.DefaultIfEmpty(), (agent, bonus) => new
            {
                agent.AgentId,
                agent.MainAgentId,
                agent.aggregate,
                bonus
            })
            .GroupBy(x => new
            {
                x.MainAgentId,
                x.AgentId
            }, (group, markets) => new
            {
                group.MainAgentId,
                group.AgentId,
                DepositAmount = markets.Sum(m => m.aggregate?.DepositAmount ?? 0M),
                WithdrawalAmount = markets.Sum(m => m.aggregate?.WithdrawalAmount ?? 0M),
                CompanyAdjustAmount = markets.Sum(m => m.aggregate?.CompanyAdjustAmount ?? 0M),
                AgentAdjustAmount = markets.Sum(m => m.aggregate?.AgentAdjustAmount ?? 0M),
                BonusAmount = markets.Sum(m => m.aggregate?.BonusAmount ?? 0M),
                BetAmount = markets.Sum(m => m.aggregate?.BetAmount ?? 0M),
                ValidBetAmount = markets.Sum(m => m.aggregate?.ValidBetAmount ?? 0M),
                Turnover = markets.Sum(m => m.aggregate?.Turnover ?? 0M),
                WinLossAmount = markets.Sum(m => m.aggregate?.WinLossAmount ?? 0M),
                NoIncludeNetProfitCompanyAdjustAmount =
                    markets.Sum(m => m.aggregate?.NoIncludeNetProfitCompanyAdjustAmount ?? 0M),
                NoIncludeNetProfitAgentAdjustAmount =
                    markets.Sum(m => m.aggregate?.NoIncludeNetProfitAgentAdjustAmount ?? 0M),
                RebateAmount = markets.Sum(m => m.aggregate?.RebateAmount ?? 0M),
                PromotionAmount = markets.Sum(m => m.aggregate?.PromotionAmount ?? 0M),
                OriginalTurnover = markets.Sum(m => m.aggregate?.OriginalTurnover ?? 0M),
                UpgradeAmount = markets.Sum(m => m.bonus?.UpgradeAmount ?? 0M),
                SeasonAmount = markets.Sum(m => m.bonus?.SeasonAmount ?? 0M),
                BirthdayAmount = markets.Sum(m => m.bonus?.BirthdayAmount ?? 0M)
            });

        var childAgentPlayers = firstLayerSubAgents.GroupJoin(
                agentPlayerAggregate,
                sub => sub.Id,
                playerAggregate => playerAggregate.MainAgentId,
                (groupAgent, groupPlayers) => new
                {
                    groupAgent,
                    groupPlayers
                })
            .SelectMany(x => x.groupPlayers.DefaultIfEmpty(),
                (x, playerAggregate) => new
                {
                    x.groupAgent,
                    playerAggregate,
                    SubAgentId = playerAggregate?.AgentId
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
                            ? 0
                            : p.playerAggregate.CompanyAdjustAmount -
                            p.playerAggregate.NoIncludeNetProfitCompanyAdjustAmount),
                        AgentAdjustAmount = playerAggregate.Sum(p => p.playerAggregate == null
                            ? 0
                            : p.playerAggregate.AgentAdjustAmount -
                            p.playerAggregate.NoIncludeNetProfitAgentAdjustAmount),
                        BonusAmount =
                            playerAggregate.Sum(p
                                => p.playerAggregate == null
                                    ? 0
                                    : p.playerAggregate.RebateAmount + p.playerAggregate.PromotionAmount +
                                    p.playerAggregate.UpgradeAmount + p.playerAggregate.SeasonAmount +
                                    p.playerAggregate.BirthdayAmount),
                        BetAmount = playerAggregate.Sum(p => p.playerAggregate?.BetAmount ?? 0),
                        ValidBetAmount = playerAggregate.Sum(p => p.playerAggregate?.OriginalTurnover ?? 0),
                        Turnover = playerAggregate.Sum(p => p.playerAggregate?.Turnover ?? 0),
                        WinLossAmount = playerAggregate.Sum(p => p.playerAggregate?.WinLossAmount ?? 0),
                        NetProfit = playerAggregate.Sum(p => p.playerAggregate == null
                            ? 0
                            : p.playerAggregate.WinLossAmount - (p.playerAggregate.AgentAdjustAmount -
                                p.playerAggregate.NoIncludeNetProfitAgentAdjustAmount) -
                            p.playerAggregate.RebateAmount -
                            p.playerAggregate.PromotionAmount -
                            p.playerAggregate.UpgradeAmount -
                            p.playerAggregate.SeasonAmount -
                            p.playerAggregate.BirthdayAmount)
                    }).ToArray();

        return childAgentPlayers.Union(players.Items ?? Array.Empty<AgentContributionModel>()).AsQueryable()
            .CreatePagedResponse(request);
    }
}