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
    private readonly TgsContext _context;

    public GetAgentContributionsQueryHandler(
        ITgsService<Repository.Entities.Agent> agentService,
        ITgsService<WithdrawForm> withdrawFormsService,
        ITgsService<Repository.Entities.Player> playerService,
        ITgsService<PlayerAggregate> playerAggregateService,
        ITgsService<PlayerBonusAggregate> playerBonusAggregate,
        TgsContext context)
    {
        _context = context;
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
        var agent = await _agentService.GetQueries()
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

        var playerWithdrawals = to > today ?
            await _withdrawFormsService.GetQueries()
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.UserType == 1 && x.Status == 9000)
            .Where(x => x.CreatedOn >= today.ToUniversalTime() && x.CreatedOn < today.AddDays(1).ToUniversalTime())
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
            .ConfigureAwait(false)
            : null;

        var playerDeposits = to > today ?
            await _context.DepositForms
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.Status == 8000)
                .Where(x => x.CreatedOn >= today.ToUniversalTime() && x.CreatedOn < today.AddDays(1).ToUniversalTime())
                .Join(_playerService.GetQueries(),
                    depositForm => depositForm.PlayerCode,
                    player => player.PlayerCode,
                    (withdrawForm, player) => new
                    {
                        player.AgentId,
                        player.PlayerCode,
                        DepositAmount = withdrawForm.ActualAmount ?? withdrawForm.Amount
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
                        DepositAmount = withdrawals.Sum(w => w.DepositAmount)
                    })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false)
            : null;

        var dailyDepositWithdraws = await _context.PlayerDailyPaymentStatistics
            .Join(_context.Players.Where(x => x.BrandId == request.BrandId), pp => pp.PlayerCode, p => p.PlayerCode,(pp,p)=>new
            {
                pp.ReportDate,
                pp.PlayerCode,
                pp.DailyDepositAmount,
                pp.DailyWithdrawAmount
            })
            .Where(x => x.ReportDate >= fromDate && x.ReportDate < toDate)
            .GroupBy(x => x.PlayerCode,(playerCode,group)=> new
            {
                PlayerCode = playerCode,
                DailyDepositAmount = group.Sum(s => s.DailyDepositAmount),
                DailyWithdrawAmount = group.Sum(s => s.DailyWithdrawAmount),
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var playersPaged = await _playerService.GetQueries()
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.AgentId == agent.Id)
            .Select(x => x.PlayerCode)
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
                        OriginalTurnover = list.Sum(p => p.OriginalTurnover),
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
                playerCode => playerCode,
                playerAggregate => playerAggregate.PlayerCode,
                (playerCode, players) => new
                {
                    PlayerCode = playerCode,
                    players
                })
            .SelectMany(x => x.players.DefaultIfEmpty(),
                (x, playerAggregate) => new
                {
                    x.PlayerCode,
                    playerAggregate.DepositAmount,
                    playerAggregate.CompanyAdjustAmount,
                    playerAggregate.AgentAdjustAmount,
                    playerAggregate.BonusAdjustAmount,
                    playerAggregate.BonusAmount,
                    playerAggregate.BetAmount,
                    playerAggregate.OriginalTurnover,
                    playerAggregate.Turnover,
                    playerAggregate.WinLossAmount,
                    playerAggregate.NetProfit
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
                    player.OriginalTurnover,
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
                    x.OriginalTurnover,
                    x.Turnover,
                    x.WinLossAmount,
                    x.NetProfit,
                    bonus.UpgradeAmount,
                    bonus.SeasonAmount,
                    bonus.BirthdayAmount
                })
            .GroupBy(x => new
                {
                    x.PlayerCode
                },
                (player, playerAggregate) =>
                    new
                    {
                        player.PlayerCode,
                        DepositAmount = playerAggregate.Sum(p => p.DepositAmount),
                        CompanyAdjustAmount = playerAggregate.Sum(p => p.CompanyAdjustAmount),
                        AgentAdjustAmount = playerAggregate.Sum(p => p.AgentAdjustAmount),
                        BonusAdjustAmount = playerAggregate.Sum(p => p.BonusAdjustAmount),
                        BonusAmount = playerAggregate.Sum(p => p.BonusAmount),
                        BetAmount = playerAggregate.Sum(p => p.BetAmount),
                        OriginalTurnover = playerAggregate.Sum(p => p.OriginalTurnover),
                        Turnover = playerAggregate.Sum(p => p.Turnover),
                        WinLossAmount = playerAggregate.Sum(p => p.WinLossAmount),
                        NetProfit = playerAggregate.Sum(p => p.NetProfit),
                        UpgradeAmount = playerAggregate.Sum(p => p.UpgradeAmount),
                        SeasonAmount = playerAggregate.Sum(p => p.SeasonAmount),
                        BirthdayAmount = playerAggregate.Sum(p => p.BirthdayAmount)
                    })
            .Join(_playerService.GetQueries(), aggregate => aggregate.PlayerCode, player => player.PlayerCode,
                (agg, player) => new
                {
                    agg.PlayerCode,
                    player.Username,
                    Type = AccountType.Player,
                    agg.DepositAmount,
                    agg.CompanyAdjustAmount,
                    agg.AgentAdjustAmount,
                    agg.BonusAdjustAmount,
                    agg.BonusAmount,
                    agg.BetAmount,
                    agg.OriginalTurnover,
                    agg.Turnover,
                    agg.WinLossAmount,
                    agg.NetProfit,
                    agg.UpgradeAmount,
                    agg.SeasonAmount,
                    agg.BirthdayAmount
                })
            .CreatePagedResponseAsync(request);

        var players = new PagedResponse<AgentContributionModel>
        {
            Valid = playersPaged.Valid,
            Message = playersPaged.Message,
            TotalCount = playersPaged.TotalCount,
            Items = playersPaged.Items?
                .GroupJoin(dailyDepositWithdraws,
                    market => market.PlayerCode,
                    deposit => deposit.PlayerCode,
                    (group, groupDepositWithdraws) => new
                    {
                        group.Username,
                        group.PlayerCode,
                        group.DepositAmount,
                        group.CompanyAdjustAmount,
                        group.AgentAdjustAmount,
                        group.BonusAdjustAmount,
                        group.BonusAmount,
                        group.BetAmount,
                        group.OriginalTurnover,
                        group.Turnover,
                        group.WinLossAmount,
                        group.NetProfit,
                        group.UpgradeAmount,
                        group.SeasonAmount,
                        group.BirthdayAmount,
                        groupDepositWithdraws
                    })
                .SelectMany(x => x.groupDepositWithdraws.DefaultIfEmpty(),
                    (x, depositWithdraw) => new
                    {
                        x.Username,
                        x.PlayerCode,
                        x.DepositAmount,
                        x.CompanyAdjustAmount,
                        x.AgentAdjustAmount,
                        x.BonusAdjustAmount,
                        x.BonusAmount,
                        x.BetAmount,
                        x.OriginalTurnover,
                        x.Turnover,
                        x.WinLossAmount,
                        x.NetProfit,
                        x.UpgradeAmount,
                        x.SeasonAmount,
                        x.BirthdayAmount,
                        depositWithdraw
                    })
                .Select(x => new AgentContributionModel
            {
                Username = x.Username,
                Type = AccountType.Player,
                DepositAmount = x.depositWithdraw?.DailyDepositAmount ?? 0M
                    + (playerDeposits?.FirstOrDefault(w => w.PlayerCode == x.PlayerCode)
                        ?.DepositAmount ?? 0M),
                WithdrawalAmount =
                    (x.depositWithdraw?.DailyWithdrawAmount ?? 0M
                        + (playerWithdrawals?.FirstOrDefault(w => w.PlayerCode == x.PlayerCode)
                            ?.WithdrawalAmount ?? 0M)),
                CompanyAdjustAmount = x.CompanyAdjustAmount,
                AgentAdjustAmount = x.AgentAdjustAmount,
                BonusAdjustAmount = x.BonusAdjustAmount,
                BonusAmount = x.BonusAmount + x.UpgradeAmount + x.SeasonAmount + x.BirthdayAmount,
                BetAmount = x.BetAmount,
                ValidBetAmount = x.OriginalTurnover,
                Turnover = x.Turnover,
                WinLossAmount = x.WinLossAmount,
                NetProfit = x.NetProfit - (x.UpgradeAmount + x.SeasonAmount + x.BirthdayAmount)
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

        var playerAggregateData = await _playerAggregateService.GetQueries()
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate >= request.From.ToUniversalTime() &&
                x.ReportDate < request.To.ToUniversalTime())
            .Where(x => x.CreatedOn >= request.From.ToUniversalTime())
            .GroupBy(x => new
            {
                x.AgentId,
                x.PlayerCode
            }, (player, list) => new AgentContributionDataModel
            {
                AgentId = player.AgentId.Value,
                PlayerCode = player.PlayerCode,
                DepositAmount = list.Sum(p => p.DepositAmount),
                CompanyAdjustAmount = list.Sum(p => p.CompanyAdjustAmount - p.NoIncludeNetProfitCompanyAdjustAmount),
                AgentAdjustAmount = list.Sum(p => p.AgentAdjustAmount - p.NoIncludeNetProfitAgentAdjustAmount),
                BonusAdjustAmount = list.Sum(p => p.BonusAdjustAmount - p.NoIncludeNetProfitBonusAdjustAmount),
                BonusAmount = list.Sum(p => p.RebateAmount + p.PromotionAmount),
                BetAmount = list.Sum(p => p.BetAmount),
                OriginalTurnover = list.Sum(p => p.OriginalTurnover),
                Turnover = list.Sum(p => p.Turnover),
                WinLossAmount = list.Sum(p => p.WinLossAmount),
                NetProfit = list.Sum(p => p.WinLossAmount
                    - (p.AgentAdjustAmount -
                        p.NoIncludeNetProfitAgentAdjustAmount)
                    - (p.BonusAdjustAmount -
                        p.NoIncludeNetProfitBonusAdjustAmount)
                    - p.RebateAmount
                    - p.PromotionAmount)
            })
            .ToArrayAsync(cancellationToken);


            var playerAggregate = playerAggregateData.GroupJoin(dailyDepositWithdraws,
                market => market.PlayerCode,
                deposit => deposit.PlayerCode,
                (group, groupDepositWithdraws) => new
                {
                    group,
                    groupDepositWithdraws
                })
            .SelectMany(x => x.groupDepositWithdraws.DefaultIfEmpty(),
                (x, depositWithdraw) => new
                {
                    x.group,
                    depositWithdraw
                })
            .GroupBy(x => new
            {
                x.group.AgentId,
                x.group.PlayerCode
            }, (player, list) => new AgentContributionDataModel
            {
                AgentId = player.AgentId,
                PlayerCode = player.PlayerCode,
                DepositAmount = list.Sum(p => p.depositWithdraw?.DailyDepositAmount ?? 0M),
                WithdrawalAmount = list.Sum(p => p.depositWithdraw?.DailyWithdrawAmount ?? 0M),
                CompanyAdjustAmount = list.Sum(p => p.group.CompanyAdjustAmount),
                AgentAdjustAmount = list.Sum(p => p.group.AgentAdjustAmount),
                BonusAdjustAmount = list.Sum(p => p.group.BonusAdjustAmount),
                BonusAmount = list.Sum(p => p.group.BonusAmount),
                BetAmount = list.Sum(p => p.group.BetAmount),
                OriginalTurnover = list.Sum(p => p.group.OriginalTurnover),
                Turnover = list.Sum(p => p.group.Turnover),
                WinLossAmount = list.Sum(p => p.group.WinLossAmount),
                NetProfit = list.Sum(p => p.group.NetProfit),
                UpgradeAmount = 0,
                SeasonAmount = 0,
                BirthdayAmount = 0
            })
            .ToArray();

        var playerBonusAggregate = await _playerBonusAggregate.GetQueries()
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate >= request.From.ToUniversalTime() &&
                x.ReportDate < request.To.ToUniversalTime())
            .GroupBy(x => new
            {
                x.AgentId,
                x.PlayerCode
            }, (player, list) => new AgentContributionDataModel
            {
                AgentId = player.AgentId.Value,
                PlayerCode = player.PlayerCode,
                DepositAmount = 0,
                WithdrawalAmount = 0,
                CompanyAdjustAmount = 0,
                AgentAdjustAmount = 0,
                BonusAdjustAmount = 0,
                BonusAmount = 0,
                BetAmount = 0,
                OriginalTurnover = 0,
                Turnover = 0,
                WinLossAmount = 0,
                NetProfit = 0,
                UpgradeAmount = list.Sum(p => p.UpgradeAmount),
                SeasonAmount = list.Sum(p => p.SeasonAmount),
                BirthdayAmount = list.Sum(p
                    => p.BirthdayAmount)
            }).ToArrayAsync(cancellationToken);

        var agentPlayerAggregate = allSubAgents
            .GroupJoin(playerAggregate.Concat<AgentContributionDataModel>(playerBonusAggregate), agent => agent.AgentId, bonus => bonus.AgentId,
                (agent, bonus) => new
                {
                    agent.AgentId,
                    agent.MainAgentId,
                    bonus
                })
            .SelectMany(x => x.bonus.DefaultIfEmpty(), (agent, bonus) => new
            {
                agent.AgentId,
                agent.MainAgentId,
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
                DepositAmount = markets.Sum(m => m.bonus?.DepositAmount ?? 0M),
                WithdrawAmount = markets.Sum(m => m.bonus?.WithdrawalAmount ?? 0M),
                CompanyAdjustAmount = markets.Sum(m => m.bonus?.CompanyAdjustAmount ?? 0M),
                BonusAdjustAmount = markets.Sum(m => m.bonus?.BonusAdjustAmount ?? 0M),
                AgentAdjustAmount = markets.Sum(m => m.bonus?.AgentAdjustAmount ?? 0M),
                BonusAmount = markets.Sum(m => m.bonus?.BonusAmount ?? 0M),
                BetAmount = markets.Sum(m => m.bonus?.BetAmount ?? 0M),
                OriginalTurnover = markets.Sum(m => m.bonus?.OriginalTurnover ?? 0M),
                Turnover = markets.Sum(m => m.bonus?.Turnover ?? 0M),
                WinLossAmount = markets.Sum(m => m.bonus?.WinLossAmount ?? 0M),
                UpgradeAmount = markets.Sum(m => m.bonus?.UpgradeAmount ?? 0M),
                SeasonAmount = markets.Sum(m => m.bonus?.SeasonAmount ?? 0M),
                BirthdayAmount = markets.Sum(m => m.bonus?.BirthdayAmount ?? 0M),
                NetProfit = markets.Sum(m => m.bonus?.NetProfit ?? 0M),
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
                        DepositAmount = playerAggregate.Sum(p => p.playerAggregate?.DepositAmount ?? 0)
                            + (playerDeposits?
                            .Where(x => x.AgentId != null)
                            .Where(w => playerAggregate.Select(p => p.SubAgentId).Contains(w.AgentId)
                                || w.AgentId != null && playerAggregate.Select(p => p.groupAgent.Id)
                                    .Contains(w.AgentId.Value))
                            .Sum(w => w.DepositAmount) ?? 0M),
                        WithdrawalAmount = playerAggregate.Sum(p => p.playerAggregate?.WithdrawAmount ?? 0)
                            + (playerWithdrawals?
                                .Where(x => x.AgentId != null)
                                .Where(w => playerAggregate.Select(p => p.SubAgentId).Contains(w.AgentId)
                                    || w.AgentId != null && playerAggregate.Select(p => p.groupAgent.Id)
                                        .Contains(w.AgentId.Value))
                                .Sum(w => w.WithdrawalAmount) ?? 0M),
                        CompanyAdjustAmount = playerAggregate.Sum(p => p.playerAggregate?.CompanyAdjustAmount ?? 0),
                        BonusAdjustAmount = playerAggregate.Sum(p => p.playerAggregate?.BonusAdjustAmount ?? 0),
                        AgentAdjustAmount = playerAggregate.Sum(p => p.playerAggregate?.AgentAdjustAmount ?? 0),
                        BonusAmount =
                            playerAggregate.Sum(p
                                => p.playerAggregate == null ? 0 : p.playerAggregate.BonusAmount +
                                    p.playerAggregate.UpgradeAmount + p.playerAggregate.SeasonAmount +
                                    p.playerAggregate.BirthdayAmount),
                        BetAmount = playerAggregate.Sum(p => p.playerAggregate?.BetAmount ?? 0),
                        ValidBetAmount = playerAggregate.Sum(p => p.playerAggregate?.OriginalTurnover ?? 0),
                        Turnover = playerAggregate.Sum(p => p.playerAggregate?.Turnover ?? 0),
                        WinLossAmount = playerAggregate.Sum(p => p.playerAggregate?.WinLossAmount ?? 0),
                        NetProfit = playerAggregate.Sum(p => p.playerAggregate == null
                            ? 0
                            : p.playerAggregate.NetProfit -
                            p.playerAggregate.UpgradeAmount -
                            p.playerAggregate.SeasonAmount -
                            p.playerAggregate.BirthdayAmount)
                    }).ToArray();

        return childAgentPlayers.Union(players.Items ?? Array.Empty<AgentContributionModel>()).AsQueryable()
            .CreatePagedResponse(request);
    }
}