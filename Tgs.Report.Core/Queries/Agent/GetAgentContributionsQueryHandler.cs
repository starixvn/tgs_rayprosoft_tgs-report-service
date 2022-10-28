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
    private readonly IAgentRepository _agentRepository;
    private readonly ITgsService<WithdrawForm> _withdrawFormsService;
    private readonly ITgsService<Repository.Entities.Player> _playerService;
    private readonly ITgsService<PlayerAggregate> _playerAggregateService;
    private readonly ITgsService<PlayerBonusAggregate> _playerBonusAggregate;
    private readonly TgsContext _context;

    public GetAgentContributionsQueryHandler(
        IAgentRepository agentRepository,
        ITgsService<WithdrawForm> withdrawFormsService,
        ITgsService<Repository.Entities.Player> playerService,
        ITgsService<PlayerAggregate> playerAggregateService,
        ITgsService<PlayerBonusAggregate> playerBonusAggregate,
        TgsContext context)
    {
        _agentRepository = agentRepository ?? throw new ArgumentNullException(nameof(agentRepository));
        _withdrawFormsService = withdrawFormsService ?? throw new ArgumentNullException(nameof(withdrawFormsService));
        _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
        _playerAggregateService = playerAggregateService ?? throw new ArgumentNullException(nameof(playerAggregateService));
        _playerBonusAggregate = playerBonusAggregate ?? throw new ArgumentNullException(nameof(playerBonusAggregate));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PagedResponse<AgentContributionModel>> Handle(
        GetAgentContributionsRequest request,
        CancellationToken cancellationToken)
    {
        var agent = await _agentRepository.GetAgentWithNLevelAsync(
            request.BrandId,
            request.AgentId,
            request.Username,
            cancellationToken).ConfigureAwait(false);

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

        var playerRealTimeWithdrawals = to > today ?
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

        var playerRealTimeDeposits = to > today ?
            await _context.DepositForms
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.Status == 8000)
                .Where(x => x.CreatedOn >= today.ToUniversalTime() && x.CreatedOn < today.AddDays(1).ToUniversalTime())
                .Join(_playerService.GetQueries(),
                    depositForm => depositForm.PlayerCode,
                    player => player.PlayerCode,
                    (depositForm, player) => new
                    {
                        player.AgentId,
                        player.PlayerCode,
                        DepositAmount = depositForm.ActualAmount ?? depositForm.Amount,
                        DepositHandlingFee = depositForm.HandlingFee
                    })
                .GroupBy(x => new
                {
                    x.AgentId,
                    x.PlayerCode
                },
                    (x, deposits) => new
                    {
                        x.AgentId,
                        x.PlayerCode,
                        DepositAmount = deposits.Sum(d => d.DepositAmount),
                        DepositHandlingFee = deposits.Sum(d => d.DepositHandlingFee)
                    })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false)
            : null;

        var playerPaymentStatistics = await _context.PlayerDailyPaymentStatistics
            .Join(_context.Players.Where(x => x.BrandId == request.BrandId), pp => pp.PlayerCode, p => p.PlayerCode, (pp, p) => new
            {
                pp.ReportDate,
                pp.PlayerCode,
                pp.DailyDepositAmount,
                pp.DailyDepositHandlingFee,
                pp.DailyWithdrawAmount
            })
            .Where(x => x.ReportDate >= fromDate && x.ReportDate < toDate)
            .GroupBy(x => x.PlayerCode, (playerCode, group) => new
            {
                PlayerCode = playerCode,
                DepositAmount = group.Sum(s => s.DailyDepositAmount),
                DepositHandlingFee = group.Sum(s => s.DailyDepositHandlingFee),
                WithdrawAmount = group.Sum(s => s.DailyWithdrawAmount),
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
                .GroupJoin(playerPaymentStatistics,
                    market => market.PlayerCode,
                    deposit => deposit.PlayerCode,
                    (group, statistics) => new
                    {
                        group.Username,
                        group.PlayerCode,
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
                        PlayerPaymentStastics = statistics
                    })
                .SelectMany(x => x.PlayerPaymentStastics.DefaultIfEmpty(),
                    (x, statistics) => new
                    {
                        x.Username,
                        x.PlayerCode,
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
                        PlayerPaymentStastics = statistics
                    })
                .Select(x =>
                {
                    var depositItem = playerRealTimeDeposits?
                        .FirstOrDefault(d => d.PlayerCode == x.PlayerCode);

                    var withdrawItem = playerRealTimeWithdrawals?
                        .FirstOrDefault(w => w.PlayerCode == x.PlayerCode);

                    return new AgentContributionModel
                    {
                        Username = x.Username,
                        Type = AccountType.Player,
                        DepositAmount = (x.PlayerPaymentStastics?.DepositAmount ?? 0M)
                                        + (depositItem?.DepositAmount ?? 0M),
                        DepositHandlingFee = (x.PlayerPaymentStastics?.DepositHandlingFee ?? 0M)
                                             + (depositItem?.DepositHandlingFee ?? 0M),
                        WithdrawalAmount =
                            (x.PlayerPaymentStastics?.WithdrawAmount ?? 0M)
                            + (withdrawItem?.WithdrawalAmount ?? 0M),
                        CompanyAdjustAmount = x.CompanyAdjustAmount,
                        AgentAdjustAmount = x.AgentAdjustAmount,
                        BonusAdjustAmount = x.BonusAdjustAmount,
                        BonusAmount = x.BonusAmount + x.UpgradeAmount + x.SeasonAmount + x.BirthdayAmount,
                        BetAmount = x.BetAmount,
                        ValidBetAmount = x.OriginalTurnover,
                        Turnover = x.Turnover,
                        WinLossAmount = x.WinLossAmount,
                        NetProfit = x.NetProfit - (x.UpgradeAmount + x.SeasonAmount + x.BirthdayAmount)
                    };
                }).ToArray()
        };

        if (!request.IncludeAgents)
        {
            return players;
        }

        var firstLayerAgents = await _agentRepository
            .GetFirstLayerAgentsAsync(request.BrandId, agent.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var allSubAgents = await _agentRepository
            .GetChildAgentsAsync(request.BrandId, agent)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var brandAgentsPaymentStatistics = await _context.AgentDailyPaymentStatistics
            .Where(x => x.ReportDate >= fromDate && x.ReportDate < toDate)
            .Where(x => x.BrandId == request.BrandId)
            .GroupBy(x => x.AgentId, (agentId, group) => new
            {
                AgentId = agentId,
                DepositAmount = group.Sum(x => x.DailyDepositAmount),
                DepositHandlingFee = group.Sum(x => x.DailyDepositHandlingFee),
                WithdrawAmount = group.Sum(x => x.DailyWithdrawAmount)
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var playerAggregateData = await _playerAggregateService.GetQueries()
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate >= request.From.ToUniversalTime() &&
                x.ReportDate < request.To.ToUniversalTime())
            .Where(x => x.CreatedOn >= request.From.ToUniversalTime())
            .Where(x => x.AgentId != null)
            .GroupBy(x => new
            {
                AgentId = x.AgentId!.Value,
                x.PlayerCode
            }, (player, aggregates) => new AgentContributionDataModel
            {
                AgentId = player.AgentId,
                PlayerCode = player.PlayerCode,
                CompanyAdjustAmount = aggregates.Sum(p => p.CompanyAdjustAmount - p.NoIncludeNetProfitCompanyAdjustAmount),
                AgentAdjustAmount = aggregates.Sum(p => p.AgentAdjustAmount - p.NoIncludeNetProfitAgentAdjustAmount),
                BonusAdjustAmount = aggregates.Sum(p => p.BonusAdjustAmount - p.NoIncludeNetProfitBonusAdjustAmount),
                BonusAmount = aggregates.Sum(p => p.RebateAmount + p.PromotionAmount),
                BetAmount = aggregates.Sum(p => p.BetAmount),
                OriginalTurnover = aggregates.Sum(p => p.OriginalTurnover),
                Turnover = aggregates.Sum(p => p.Turnover),
                WinLossAmount = aggregates.Sum(p => p.WinLossAmount),
                NetProfit = aggregates.Sum(p => p.WinLossAmount
                    - (p.BonusAdjustAmount -
                        p.NoIncludeNetProfitBonusAdjustAmount)
                    - p.RebateAmount
                    - p.PromotionAmount)
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var playerBonusAggregate = await _playerBonusAggregate.GetQueries()
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate >= request.From.ToUniversalTime() &&
                x.ReportDate < request.To.ToUniversalTime())
            .Where(x => x.AgentId != null)
            .GroupBy(x => new
            {
                AgentId = x.AgentId!.Value,
                x.PlayerCode
            }, (player, list) => new AgentContributionDataModel
            {
                AgentId = player.AgentId,
                PlayerCode = player.PlayerCode,
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
            .GroupJoin(playerAggregateData.Concat(playerBonusAggregate), agent => agent.AgentId, data => data.AgentId,
                (agent, contributions) => new
                {
                    agent.AgentId,
                    agent.FirstLayerAgentId,
                    Contributions = contributions
                })
            .SelectMany(x => x.Contributions.DefaultIfEmpty(),
                (agent, contribution) => new
                {
                    agent.AgentId,
                    agent.FirstLayerAgentId,
                    Contribution = contribution
                })
            .GroupJoin(brandAgentsPaymentStatistics,
                agent => agent.AgentId,
                paymentStatistics => paymentStatistics.AgentId,
                (agent, statistics) => new
                {
                    agent.AgentId,
                    agent.FirstLayerAgentId,
                    Contribution = agent.Contribution,
                    PaymentStatistics = statistics
                })
            .SelectMany(x => x.PaymentStatistics.DefaultIfEmpty(),
                (agent, statistics) => new
                {
                    agent.AgentId,
                    agent.FirstLayerAgentId,
                    Contribution = agent.Contribution,
                    DepositAmount = statistics?.DepositAmount,
                    DepositHandlingFee = statistics?.DepositHandlingFee,
                    WithdrawAmount = statistics?.WithdrawAmount
                })
            .GroupBy(x => new

            {
                x.FirstLayerAgentId,
                x.AgentId
            }, (group, markets) => new
            {
                group.FirstLayerAgentId,
                group.AgentId,
                DepositAmount = markets.Sum(m => m.DepositAmount ?? 0M),
                DepositHandlingFee = markets.Sum(m => m.DepositHandlingFee ?? 0M),
                WithdrawAmount = markets.Sum(m => m.WithdrawAmount ?? 0M),
                CompanyAdjustAmount = markets.Sum(m => m.Contribution?.CompanyAdjustAmount ?? 0M),
                BonusAdjustAmount = markets.Sum(m => m.Contribution?.BonusAdjustAmount ?? 0M),
                AgentAdjustAmount = markets.Sum(m => m.Contribution?.AgentAdjustAmount ?? 0M),
                BonusAmount = markets.Sum(m => m.Contribution?.BonusAmount ?? 0M),
                BetAmount = markets.Sum(m => m.Contribution?.BetAmount ?? 0M),
                OriginalTurnover = markets.Sum(m => m.Contribution?.OriginalTurnover ?? 0M),
                Turnover = markets.Sum(m => m.Contribution?.Turnover ?? 0M),
                WinLossAmount = markets.Sum(m => m.Contribution?.WinLossAmount ?? 0M),
                UpgradeAmount = markets.Sum(m => m.Contribution?.UpgradeAmount ?? 0M),
                SeasonAmount = markets.Sum(m => m.Contribution?.SeasonAmount ?? 0M),
                BirthdayAmount = markets.Sum(m => m.Contribution?.BirthdayAmount ?? 0M),
                NetProfit = markets.Sum(m => m.Contribution?.NetProfit ?? 0M),
            });

        var childAgentPlayers = firstLayerAgents
            .GroupJoin(
                agentPlayerAggregate,
                agent => agent.Id,
                playerAggregate => playerAggregate.FirstLayerAgentId,
                (firstLayerAgent, agentPlayers) => new
                {
                    FirstLayerAgent = firstLayerAgent,
                    AgentPlayerAggregates = agentPlayers
                })
            .SelectMany(x => x.AgentPlayerAggregates.DefaultIfEmpty(),
                (x, playerAggregate) => new
                {
                    FirstLayerAgent = x.FirstLayerAgent,
                    AgentPlayerAggregate = playerAggregate,
                    SubAgentId = playerAggregate?.AgentId
                })
            .GroupBy(x => new
            {
                x.FirstLayerAgent.UserName,
                x.FirstLayerAgent.Id
            },
                (firstLayerAgent, aggregates) =>
                {
                    var agentIds = aggregates
                        .Select(p => p.FirstLayerAgent.Id)
                        .Concat(aggregates
                            .Where(p => p.SubAgentId.HasValue)
                            .Select(p => p.SubAgentId!.Value))
                        .Distinct();

                    var realTimeDeposits = playerRealTimeDeposits?
                        .Where(x => x.AgentId != null)
                        .Where(x => agentIds.Contains(x.AgentId!.Value));

                    var realTimeWithdraws = playerRealTimeWithdrawals?
                        .Where(x => x.AgentId != null)
                        .Where(x => agentIds.Contains(x.AgentId!.Value));

                    return new AgentContributionModel
                    {
                        Username = firstLayerAgent.UserName,
                        Type = AccountType.Agent,
                        DepositAmount = aggregates.Sum(p
                                            => p.AgentPlayerAggregate?.DepositAmount ?? 0M)
                                        + (realTimeDeposits?.Sum(w => w.DepositAmount) ?? 0M),
                        DepositHandlingFee = aggregates.Sum(p
                                                 => p.AgentPlayerAggregate?.DepositHandlingFee ?? 0M)
                                             + (realTimeDeposits?.Sum(w => w.DepositHandlingFee) ?? 0M),
                        WithdrawalAmount = aggregates.Sum(p
                                               => p.AgentPlayerAggregate?.WithdrawAmount ?? 0M)
                                           + (realTimeWithdraws?.Sum(w => w.WithdrawalAmount) ?? 0M),
                        CompanyAdjustAmount = aggregates.Sum(p => p.AgentPlayerAggregate?.CompanyAdjustAmount ?? 0),
                        BonusAdjustAmount = aggregates.Sum(p => p.AgentPlayerAggregate?.BonusAdjustAmount ?? 0),
                        AgentAdjustAmount = aggregates.Sum(p => p.AgentPlayerAggregate?.AgentAdjustAmount ?? 0),
                        BonusAmount =
                                aggregates.Sum(p
                                    => p.AgentPlayerAggregate == null
                                        ? 0
                                        : p.AgentPlayerAggregate.BonusAmount +
                                          p.AgentPlayerAggregate.UpgradeAmount + p.AgentPlayerAggregate.SeasonAmount +
                                          p.AgentPlayerAggregate.BirthdayAmount),
                        BetAmount = aggregates.Sum(p => p.AgentPlayerAggregate?.BetAmount ?? 0),
                        ValidBetAmount = aggregates.Sum(p => p.AgentPlayerAggregate?.OriginalTurnover ?? 0),
                        Turnover = aggregates.Sum(p => p.AgentPlayerAggregate?.Turnover ?? 0),
                        WinLossAmount = aggregates.Sum(p => p.AgentPlayerAggregate?.WinLossAmount ?? 0),
                        NetProfit = aggregates.Sum(p => p.AgentPlayerAggregate == null
                            ? 0
                            : p.AgentPlayerAggregate.NetProfit -
                              p.AgentPlayerAggregate.UpgradeAmount -
                              p.AgentPlayerAggregate.SeasonAmount -
                              p.AgentPlayerAggregate.BirthdayAmount)
                    };
                }).ToArray();

        return childAgentPlayers
            .Union(players.Items ?? Array.Empty<AgentContributionModel>())
            .AsQueryable()
            .CreatePagedResponse(request);
    }
}