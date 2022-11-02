using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Market;

namespace Tgs.Report.Core.Queries.Market;

public class GetMarketsQueryHandler : IRequestHandler<GetMarketsRequest, PagedResponse<MarketModel>>
{
    private readonly TgsContext _context;

    public GetMarketsQueryHandler(TgsContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<PagedResponse<MarketModel>> Handle(
        GetMarketsRequest request,
        CancellationToken cancellationToken)
    {
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
            requestTo.Offset);

        var dates = Enumerable.Range(0, to.Subtract(from).Days)
            .Select(x => from.AddDays(x))
            .ToArray();

        var withdrawals = to > today ?
            await _context.WithdrawForms
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.UserType == 1 && x.Status == 9000)
            .Where(x => x.CreatedOn >= today.ToUniversalTime() && x.CreatedOn < today.AddDays(1).ToUniversalTime())
            .Select(x => new
            {
                x.CreatedOn,
                x.ActualAmount,
                x.Amount
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false) : null;

        var withdrawalByDates = withdrawals?.GroupBy(x => x.CreatedOn.ToOffset(offset).Date,
            (date, withdrawals) => new
            {
                Date = date,
                WithdrawalAmount = withdrawals.Sum(w => w.ActualAmount ?? w.Amount)
            });

        var deposits = to > today ?
            await _context.DepositForms
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.Status == 8000)
                .Where(x => x.CreatedOn >= today.ToUniversalTime()
                            && x.CreatedOn < today.AddDays(1).ToUniversalTime())
                .Select(x => new
                {
                    x.CreatedOn,
                    x.ActualAmount,
                    x.Amount,
                    x.HandlingFee
                })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false) : null;

        var depositByDates = deposits?
            .GroupBy(x => x.CreatedOn.ToOffset(offset).Date,
            (date, deposits) => new
            {
                Date = date,
                DepositAmount = deposits.Sum(d => d.ActualAmount ?? d.Amount),
                DepositHandlingFee = deposits.Sum(d => d.HandlingFee)
            });

        var playerLoginRecords = await _context.PlayerLoginRecords
            .Where(plr => plr.BrandId == request.BrandId)
            .Where(plr => plr.CreatedOn >= from.ToUniversalTime() && plr.CreatedOn < to.ToUniversalTime())
            .Select(plr => new
            {
                plr.CreatedOn,
                plr.PlayerCode
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var playerLoginRecordsByDates = playerLoginRecords.GroupBy(x => x.CreatedOn.ToOffset(offset).Date,
            (date, playerLoginRecords) => new
            {
                Date = date,
                LoginPlayer = playerLoginRecords.GroupBy(x => x.PlayerCode).Sum(plr => 1)
            });

        var fromDate = new DateOnly(from.Year, from.Month, from.Day);
        var toDate = new DateOnly(to.Year, to.Month, to.Day);

        var playerRegisterCount = to > today ? await _context.Players
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.RegisteredOn >= today.ToUniversalTime()
                && x.RegisteredOn < today.AddDays(1).ToUniversalTime())
            .CountAsync(cancellationToken)
            .ConfigureAwait(false) : 0;

        var playerDailyRegisterStatistics = await _context.PlayerDailyRegisterStatistics
            .Join(_context.Players,
                pdp => pdp.PlayerCode,
                p => p.PlayerCode, (agg, p) => new
                {
                    agg,
                    p
                })
            .Where(x => x.p.BrandId == request.BrandId)
            .Where(x => x.agg.ReportDate >= fromDate && x.agg.ReportDate < toDate)
            .Select(x => new
            {
                x.agg.ReportDate,
                x.agg.PlayerCode
            })
            .GroupBy(x => x.ReportDate, (reportDate, players) => new
            {
                ReportDate = reportDate,
                PlayerCount = players.Count()
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var brandPaymentStatistics = await _context.BrandDailyPaymentStatistics
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate >= fromDate && x.ReportDate < toDate)
            .Select(x => new
            {
                x.ReportDate,
                x.FirstDepositPlayers,
                x.DailyDepositAmount,
                x.DailyDepositHandlingFee,
                x.DailyWithdrawAmount
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var activePlayerAggregates = await _context.PlayerAggregates
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
            .Where(x => x.CreatedOn >= from.ToUniversalTime())
            .GroupBy(x => new
            {
                x.ReportDate,
                x.PlayerCode
            }, (x, group) => new
            {
                x.ReportDate,
                x.PlayerCode,
                DepositAmount = group.Sum(s => s.DepositAmount),
                BetAmount = group.Sum(s => s.BetAmount)
            })
            .GroupBy(x => x.ReportDate, (reportDate, group) => new
            {
                ReportDate = reportDate,
                DepositCount = group.Sum(s => s.DepositAmount > 0 ? 1 : 0),
                ActivePlayer = group.Sum(s => s.BetAmount > 0 ? 1 : 0)
            }).ToArrayAsync(cancellationToken);

        var agentAggregates = await _context.PlayerAggregates
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
            .Where(x => x.CreatedOn >= from.ToUniversalTime())
            .GroupBy(x => new
            {
                x.ReportDate,
                x.Hour,
                x.AgentId,
                x.ChiefAgentDividendPercentage
            }, (x, aggregates) => new MarketHourlyModel
            {
                ReportDate = x.ReportDate,
                Hour = x.Hour,
                AgentId = x.AgentId,
                ChiefAgentDividendPercentage = x.ChiefAgentDividendPercentage,
                RegisteredCount = aggregates.Sum(s => s.RegisteredCount),
                CompanyAdjustAmount = aggregates.Sum(s => s.CompanyAdjustAmount),
                NoIncludeNetProfitCompanyAdjustAmount = aggregates.Sum(s => s.NoIncludeNetProfitCompanyAdjustAmount),
                BonusAdjustAmount = aggregates.Sum(s => s.BonusAdjustAmount),
                NoIncludeNetProfitBonusAdjustAmount = aggregates.Sum(s => s.NoIncludeNetProfitBonusAdjustAmount),
                RebateAmount = aggregates.Sum(s => s.RebateAmount),
                PromotionAmount = aggregates.Sum(s => s.PromotionAmount),
                BetAmount = aggregates.Sum(s => s.BetAmount),
                OriginalTurnover = aggregates.Sum(s => s.OriginalTurnover),
                Turnover = aggregates.Sum(s => s.Turnover),
                WinLossAmount = aggregates.Sum(s => s.WinLossAmount),
                AllAgentRebateAmount = aggregates.Sum(s => s.AllAgentRebateAmount)
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var agentBonusAggregate = await _context.PlayerBonusAggregates
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate >= from.ToUniversalTime()
                        && x.ReportDate < to.ToUniversalTime())
            .GroupBy(x => new
            {
                x.ReportDate,
                x.Hour,
                x.AgentId
            }, (x, aggregates) => new MarketHourlyModel
            {
                ReportDate = x.ReportDate,
                Hour = x.Hour,
                AgentId = x.AgentId,
                RegisteredCount = 0,
                CompanyAdjustAmount = 0M,
                NoIncludeNetProfitCompanyAdjustAmount = 0M,
                BonusAdjustAmount = 0M,
                NoIncludeNetProfitBonusAdjustAmount = 0M,
                BetAmount = 0M,
                OriginalTurnover = 0M,
                Turnover = 0M,
                WinLossAmount = 0M,
                AllAgentRebateAmount = 0M,
                RebateAmount = 0M,
                PromotionAmount = 0M,
                UpgradeAmount = aggregates.Sum(s => s.UpgradeAmount),
                SeasonAmount = aggregates.Sum(s => s.SeasonAmount),
                BirthdayAmount = aggregates.Sum(s => s.BirthdayAmount)
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var aggregates = agentAggregates
            .GroupJoin(agentBonusAggregate,
                agent => new { agent.ReportDate, agent.Hour, agent.AgentId },
                bonus => new { bonus.ReportDate, bonus.Hour, bonus.AgentId },
                (agent, bonus) => new
                {
                    AgentAggregate = agent,
                    BonusAggregates = bonus
                })
            .SelectMany(x => x.BonusAggregates.DefaultIfEmpty(),
                (x, bonusAggregate) => new
                {
                    x.AgentAggregate,
                    BonusAggregate = bonusAggregate
                })
            .GroupBy(x => new
            {
                x.AgentAggregate.ReportDate,
                x.AgentAggregate.Hour,
                x.AgentAggregate.AgentId,
                x.AgentAggregate.ChiefAgentDividendPercentage
            }, (groupItem, aggregates) => new
            {
                groupItem.ReportDate,
                groupItem.Hour,
                groupItem.AgentId,
                groupItem.ChiefAgentDividendPercentage,
                RegisteredCount = aggregates.Sum(g => g.AgentAggregate.RegisteredCount),
                CompanyAdjustAmount = aggregates.Sum(g => g.AgentAggregate.CompanyAdjustAmount),
                NoIncludeNetProfitCompanyAdjustAmount = aggregates.Sum(g => g.AgentAggregate.NoIncludeNetProfitCompanyAdjustAmount),
                BonusAdjustAmount = aggregates.Sum(g => g.AgentAggregate.BonusAdjustAmount),
                NoIncludeNetProfitBonusAdjustAmount = aggregates.Sum(g => g.AgentAggregate.NoIncludeNetProfitBonusAdjustAmount),
                RebateAmount = aggregates.Sum(g => g.AgentAggregate.RebateAmount),
                PromotionAmount = aggregates.Sum(g => g.AgentAggregate.PromotionAmount),
                BetAmount = aggregates.Sum(g => g.AgentAggregate.BetAmount),
                OriginalTurnover = aggregates.Sum(g => g.AgentAggregate.OriginalTurnover),
                Turnover = aggregates.Sum(g => g.AgentAggregate.Turnover),
                WinLossAmount = aggregates.Sum(g => g.AgentAggregate.WinLossAmount),
                AllAgentRebateAmount = aggregates.Sum(g => g.AgentAggregate.AllAgentRebateAmount),
                UpgradeAmount = aggregates.Sum(g => g.BonusAggregate?.UpgradeAmount ?? 0M),
                SeasonAmount = aggregates.Sum(g => g.BonusAggregate?.SeasonAmount ?? 0M),
                BirthdayAmount = aggregates.Sum(g => g.BonusAggregate?.BirthdayAmount ?? 0M),
            })
            .GroupBy(x => x.ReportDate, (reportDate, group) => new
            {
                ReportDate = reportDate,
                RegisteredCount = group.Sum(g => g.RegisteredCount),
                CompanyAdjustAmount = group.Sum(g => g.CompanyAdjustAmount),
                NoIncludeNetProfitCompanyAdjustAmount = group.Sum(g => g.NoIncludeNetProfitCompanyAdjustAmount),
                BonusAdjustAmount = group.Sum(g => g.BonusAdjustAmount),
                NoIncludeNetProfitBonusAdjustAmount = group.Sum(g => g.NoIncludeNetProfitBonusAdjustAmount),
                RebateAmount = group.Sum(g => g.RebateAmount),
                PromotionAmount = group.Sum(g => g.PromotionAmount),
                BetAmount = group.Sum(g => g.BetAmount),
                OriginalTurnover = group.Sum(g => g.OriginalTurnover),
                Turnover = group.Sum(g => g.Turnover),
                WinLossAmount = group.Sum(g => g.WinLossAmount),
                AllAgentRebateAmount = group.Sum(g => g.AllAgentRebateAmount),
                UpgradeAmount = group.Sum(m => m.UpgradeAmount),
                SeasonAmount = group.Sum(m => m.SeasonAmount),
                BirthdayAmount = group.Sum(m => m.BirthdayAmount),
                NetProfit = group.Sum(m
                    => m.WinLossAmount -
                    (m.CompanyAdjustAmount - m.NoIncludeNetProfitCompanyAdjustAmount) -
                    (m.ChiefAgentDividendPercentage == null
                        ? 0M
                        :
                        (m.RebateAmount + m.PromotionAmount +
                            m.UpgradeAmount + m.SeasonAmount + m.BirthdayAmount +
                            m.BonusAdjustAmount - m.NoIncludeNetProfitBonusAdjustAmount) * ((100 - m.ChiefAgentDividendPercentage.Value) / 100))),
                PlatformCoveredBonus =
                    group.Sum(
                        m => m.ChiefAgentDividendPercentage == null
                            ? 0M
                            : (m.RebateAmount + m.PromotionAmount + m.UpgradeAmount + m.SeasonAmount + m.BirthdayAmount) * ((100 - m.ChiefAgentDividendPercentage.Value) / 100)),
                PlatformCoveredBonusAdjust =
                    group.Sum(m => m.ChiefAgentDividendPercentage == null
                        ? 0M
                        : (m.BonusAdjustAmount - m.NoIncludeNetProfitBonusAdjustAmount) * ((100 - m.ChiefAgentDividendPercentage.Value) / 100))
            })
            .ToArray();

        return dates.GroupJoin(aggregates,
                date => date,
                market => market.ReportDate,
                (date, markets) => new
                {
                    ReportDate = date,
                    MarketAggregates = markets
                })
            .SelectMany(x => x.MarketAggregates.DefaultIfEmpty(),
                (x, market) => new
                {
                    x.ReportDate,
                    MarketAggregate = market
                })
            .GroupJoin(brandPaymentStatistics,
                market => new(market.ReportDate.Year, market.ReportDate.Month, market.ReportDate.Day),
                deposit => deposit.ReportDate,
                (group, paymentStatistics) => new
                {
                    group.ReportDate,
                    group.MarketAggregate,
                    PaymentStatistics = paymentStatistics
                })
            .SelectMany(x => x.PaymentStatistics.DefaultIfEmpty(),
                (x, paymentStatistic) => new
                {
                    x.ReportDate,
                    x.MarketAggregate,
                    PaymentStatistic = paymentStatistic
                })
            .GroupJoin(activePlayerAggregates,
                market => market.ReportDate,
                player => player.ReportDate,
                (x, activePlayers) => new
                {
                    x.ReportDate,
                    x.MarketAggregate,
                    x.PaymentStatistic,
                    ActivePlayers = activePlayers
                })
            .SelectMany(x => x.ActivePlayers.DefaultIfEmpty(),
                (x, activePlayer) => new
                {
                    x.ReportDate,
                    x.MarketAggregate,
                    x.PaymentStatistic,
                    activePlayer?.DepositCount,
                    activePlayer?.ActivePlayer
                })
            .GroupBy(x => x.ReportDate,
                (reportDate, aggregates) =>
                {
                    var realTimeDeposit = depositByDates?.FirstOrDefault(d => d.Date == reportDate.Date);
                    var realTimeWithdraw = withdrawalByDates?.FirstOrDefault(d => d.Date == reportDate.Date);

                    return new MarketModel
                    {
                        ReportDate = reportDate,
                        DepositCount = aggregates.Sum(m => m.DepositCount ?? 0),
                        ActivePlayer = aggregates.Sum(m => m.ActivePlayer ?? 0),
                        LoginPlayer =
                            playerLoginRecordsByDates.FirstOrDefault(plr => plr.Date == reportDate.Date)
                                ?.LoginPlayer ?? 0,
                        RegisteredCount = (playerDailyRegisterStatistics?.FirstOrDefault(pdr => pdr.ReportDate == new DateOnly(reportDate.Year, reportDate.Month, reportDate.Day))?.PlayerCount ?? 0)
                        + (reportDate == today ? playerRegisterCount : 0),
                        FirstFundInCount = aggregates.Sum(m => m.PaymentStatistic?.FirstDepositPlayers ?? 0),
                        DepositAmount = aggregates.Sum(m => m.PaymentStatistic?.DailyDepositAmount ?? 0M)
                                        + (realTimeDeposit?.DepositAmount ?? 0M),
                        DepositHandlingFee = aggregates.Sum(m => m.PaymentStatistic?.DailyDepositHandlingFee ?? 0M) + (realTimeDeposit?.DepositHandlingFee ?? 0M),
                        WithdrawalAmount =
                            aggregates.Sum(m => m.PaymentStatistic?.DailyWithdrawAmount ?? 0M)
                            + (realTimeWithdraw?.WithdrawalAmount ?? 0M),
                        CompanyAdjustAmount = aggregates.Sum(m
                            => m.MarketAggregate == null
                                ? 0M
                                : m.MarketAggregate.CompanyAdjustAmount - m.MarketAggregate.NoIncludeNetProfitCompanyAdjustAmount),
                        BonusAdjustAmount = aggregates.Sum(m
                            => m.MarketAggregate == null
                                ? 0M
                                : m.MarketAggregate.BonusAdjustAmount - m.MarketAggregate.NoIncludeNetProfitBonusAdjustAmount),
                        BonusAmount =
                            aggregates.Sum(
                                m => m.MarketAggregate == null
                                    ? 0
                                    : m.MarketAggregate.RebateAmount
                                      + m.MarketAggregate.PromotionAmount
                                      + m.MarketAggregate.UpgradeAmount
                                      + m.MarketAggregate.SeasonAmount
                                      + m.MarketAggregate.BirthdayAmount),
                        BetAmount = aggregates.Sum(m => m.MarketAggregate?.BetAmount ?? 0M),
                        ValidBetAmount = aggregates.Sum(m => m.MarketAggregate?.OriginalTurnover ?? 0M),
                        Turnover = aggregates.Sum(m => m.MarketAggregate?.Turnover ?? 0M),
                        WinLossAmount = aggregates.Sum(m => m.MarketAggregate?.WinLossAmount ?? 0M),
                        NetProfit = aggregates.Sum(m => m.MarketAggregate?.NetProfit ?? 0M),
                        AdditionalExpenses = aggregates.Sum(m => m.MarketAggregate?.AllAgentRebateAmount ?? 0M),
                        PlatformCoveredBonus = aggregates.Sum(m => m.MarketAggregate?.PlatformCoveredBonus ?? 0M),
                        PlatformCoveredBonusAdjust = aggregates.Sum(m => m.MarketAggregate?.PlatformCoveredBonusAdjust ?? 0M)
                    };
                })
            .AsQueryable()
            .CreatePagedResponse(request);
    }
}