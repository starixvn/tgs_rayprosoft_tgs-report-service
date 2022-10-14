using System.Linq;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
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
                .Where(x => x.CreatedOn >= today.ToUniversalTime() && x.CreatedOn < today.AddDays(1).ToUniversalTime())
                .Select(x => new
                {
                    x.CreatedOn,
                    x.ActualAmount,
                    x.Amount
                })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false) : null;

        var depositByDates = deposits?.GroupBy(x => x.CreatedOn.ToOffset(offset).Date,
            (date, deposits) => new
            {
                Date = date,
                DepositAmount = deposits.Sum(w => w.ActualAmount ?? w.Amount)
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

        var dailyFirstDeposits = await _context.BrandDailyPaymentStatistics
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate >= fromDate && x.ReportDate < toDate)
            .Select(x => new
            {
                x.ReportDate,
                x.FirstDepositPlayers,
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var dailyDepositWithdraws = await _context.PlayerDailyPaymentStatistics
            .Join(_context.Players.Where(x => x.BrandId == request.BrandId), pp => pp.PlayerCode, p => p.PlayerCode,(pp,p)=>new
            {
                pp.ReportDate,
                pp.DailyDepositAmount,
                pp.DailyWithdrawAmount
            })
            .Where(x => x.ReportDate >= fromDate && x.ReportDate < toDate)
            .GroupBy(x => x.ReportDate,(reportDate,group)=> new
            {
                ReportDate = reportDate,
                DailyDepositAmount = group.Sum(s => s.DailyDepositAmount),
                DailyWithdrawAmount = group.Sum(s => s.DailyWithdrawAmount),
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var active = await _context.PlayerAggregates
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
                BetAmount = group.Sum(s => s.BetAmount),
            })
            .GroupBy(x => x.ReportDate, (reportDate, group) => new
            {
                ReportDate = reportDate,
                DepositCount = group.Sum(s => s.DepositAmount > 0 ? 1 : 0),
                ActivePlayer = group.Sum(s => s.BetAmount > 0 ? 1 : 0)
            }).ToArrayAsync(cancellationToken);

        var agentAggregate = await _context.PlayerAggregates
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
            .Where(x => x.CreatedOn >= from.ToUniversalTime())
            .GroupBy(x => new
            {
                x.ReportDate,
                x.Hour,
                x.AgentId,
                x.ChiefAgentDividendPercentage
            }, (x, group) => new MarketHourlyModel
            {
                ReportDate = x.ReportDate,
                Hour = x.Hour,
                AgentId = x.AgentId,
                ChiefAgentDividendPercentage = x.ChiefAgentDividendPercentage,
                RegisteredCount = group.Sum(s => s.RegisteredCount),
                FirstFundInCount = group.Sum(s => s.FirstFundInCount),
                CompanyAdjustAmount = group.Sum(s => s.CompanyAdjustAmount),
                NoIncludeNetProfitCompanyAdjustAmount = group.Sum(s => s.NoIncludeNetProfitCompanyAdjustAmount),
                BonusAdjustAmount = group.Sum(s => s.BonusAdjustAmount),
                NoIncludeNetProfitBonusAdjustAmount = group.Sum(s => s.NoIncludeNetProfitBonusAdjustAmount),
                RebateAmount = group.Sum(s => s.RebateAmount),
                PromotionAmount = group.Sum(s => s.PromotionAmount),
                BetAmount = group.Sum(s => s.BetAmount),
                OriginalTurnover = group.Sum(s => s.OriginalTurnover),
                Turnover = group.Sum(s => s.Turnover),
                WinLossAmount = group.Sum(s => s.WinLossAmount),
                AllAgentRebateAmount = group.Sum(s => s.AllAgentRebateAmount)
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var agentBonusAggregate = await _context.PlayerBonusAggregates
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
            .GroupBy(x => new {
                x.ReportDate,
                x.Hour,
                x.AgentId

            }, (x, group) => new MarketHourlyModel
            {
                ReportDate = x.ReportDate,
                Hour = x.Hour,
                AgentId = x.AgentId,
                RegisteredCount = 0,
                FirstFundInCount = 0,
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
                UpgradeAmount = group.Sum(s => s.UpgradeAmount),
                SeasonAmount = group.Sum(s => s.SeasonAmount),
                BirthdayAmount = group.Sum(s => s.BirthdayAmount)
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var aggregate = agentAggregate
            .GroupJoin(agentBonusAggregate,
                a => new { a.ReportDate, a.Hour, a.AgentId },
                ab => new { ab.ReportDate, ab.Hour, ab.AgentId },
                (agg, list) => new
                {
                    agg,
                    list
                })
            .SelectMany(x => x.list.DefaultIfEmpty(),
                (x, bonusAgg) => new
                {
                    x.agg,
                    bonusAgg
                })
            .GroupBy(x => new
            {
                x.agg.ReportDate,
                x.agg.Hour,
                x.agg.AgentId,
                x.agg.ChiefAgentDividendPercentage
            },(groupItem, list) => new
            {
                groupItem.ReportDate,
                groupItem.Hour,
                groupItem.AgentId,
                groupItem.ChiefAgentDividendPercentage,
                RegisteredCount = list.Sum(g => g.agg.RegisteredCount),
                FirstFundInCount = list.Sum(g => g.agg.FirstFundInCount),
                CompanyAdjustAmount = list.Sum(g => g.agg.CompanyAdjustAmount),
                NoIncludeNetProfitCompanyAdjustAmount = list.Sum(g => g.agg.NoIncludeNetProfitCompanyAdjustAmount),
                BonusAdjustAmount = list.Sum(g => g.agg.BonusAdjustAmount),
                NoIncludeNetProfitBonusAdjustAmount = list.Sum(g => g.agg.NoIncludeNetProfitBonusAdjustAmount),
                RebateAmount = list.Sum(g => g.agg.RebateAmount),
                PromotionAmount = list.Sum(g => g.agg.PromotionAmount),
                BetAmount = list.Sum(g => g.agg.BetAmount),
                OriginalTurnover = list.Sum(g => g.agg.OriginalTurnover),
                Turnover = list.Sum(g => g.agg.Turnover),
                WinLossAmount = list.Sum(g => g.agg.WinLossAmount),
                AllAgentRebateAmount = list.Sum(g => g.agg.AllAgentRebateAmount),
                UpgradeAmount = list.Sum(m => m.bonusAgg?.UpgradeAmount ?? 0M),
                SeasonAmount = list.Sum(m => m.bonusAgg?.SeasonAmount ?? 0M),
                BirthdayAmount = list.Sum(m => m.bonusAgg?.BirthdayAmount ?? 0M),
            })
            .GroupBy(x => x.ReportDate, (reportDate, group) => new
            {
                ReportDate = reportDate,
                RegisteredCount = group.Sum(g => g.RegisteredCount),
                FirstFundInCount = group.Sum(g => g.FirstFundInCount),
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

        return dates.GroupJoin(aggregate,
                date => date,
                market => market.ReportDate,
                (date, markets) => new
                {
                    date,
                    markets
                })
            .SelectMany(x => x.markets.DefaultIfEmpty(),
                (x, market) => new
                {
                    ReportDate = x.date,
                    market
                })
            .GroupJoin(dailyFirstDeposits,
                market => new(market.ReportDate.Year, market.ReportDate.Month, market.ReportDate.Day),
                deposit => deposit.ReportDate,
                (group, groupDeposits) => new
                {
                    group.ReportDate,
                    group.market,
                    groupDeposits
                })
            .SelectMany(x => x.groupDeposits.DefaultIfEmpty(),
                (x, firstDeposit) => new
                {
                    x.ReportDate,
                    x.market,
                    firstDeposit
                })
            .GroupJoin(dailyDepositWithdraws,
                market => new(market.ReportDate.Year, market.ReportDate.Month, market.ReportDate.Day),
                deposit => deposit.ReportDate,
                (group, groupDepositWithdraws) => new
                {
                    group.ReportDate,
                    group.market,
                    group.firstDeposit,
                    groupDepositWithdraws
                })
            .SelectMany(x => x.groupDepositWithdraws.DefaultIfEmpty(),
                (x, depositWithdraw) => new
                {
                    x.ReportDate,
                    x.market,
                    x.firstDeposit,
                    depositWithdraw
                })
            .GroupJoin(active,
                market => market.ReportDate,
                act => act.ReportDate,
                (x, acts) => new
                {
                    x.ReportDate,
                    x.market,
                    x.firstDeposit,
                    x.depositWithdraw,
                    acts
                })
            .SelectMany(x => x.acts.DefaultIfEmpty(),
                (x, act) => new
                {
                    x.ReportDate,
                    x.market,
                    x.firstDeposit,
                    x.depositWithdraw,
                    x.acts,
                    act
                })
            .GroupBy(x => new { x.ReportDate },
                (market, markets) =>
                    new MarketModel
                    {
                        ReportDate = market.ReportDate,
                        DepositCount = markets.Sum(m => m.act?.DepositCount ?? 0),
                        ActivePlayer = markets.Sum(m => m.act?.ActivePlayer ?? 0),
                        LoginPlayer =
                            playerLoginRecordsByDates.FirstOrDefault(plr => plr.Date == market.ReportDate.Date)
                                ?.LoginPlayer ?? 0,
                        RegisteredCount = markets.Sum(m => m.market?.RegisteredCount ?? 0),
                        FirstFundInCount = markets.Sum(m => m.firstDeposit?.FirstDepositPlayers ?? 0),
                        DepositAmount = markets.Sum(m => m.depositWithdraw?.DailyDepositAmount ?? 0M)
                        + (depositByDates?.FirstOrDefault(w => w.Date == market.ReportDate.Date)
                            ?.DepositAmount ?? 0M),
                        WithdrawalAmount =
                            (markets.Sum(m => m.depositWithdraw?.DailyWithdrawAmount ?? 0M)
                                + (withdrawalByDates?.FirstOrDefault(w => w.Date == market.ReportDate.Date)
                                    ?.WithdrawalAmount ?? 0M)),
                        CompanyAdjustAmount = markets.Sum(m
                            => m.market == null
                                ? 0M
                                : m.market.CompanyAdjustAmount - m.market.NoIncludeNetProfitCompanyAdjustAmount),
                        BonusAdjustAmount = markets.Sum(m
                            => m.market == null
                                ? 0M
                                : m.market.BonusAdjustAmount - m.market.NoIncludeNetProfitBonusAdjustAmount),
                        BonusAmount =
                            markets.Sum(
                                m => m.market == null
                                    ? 0
                                    : m.market.RebateAmount + m.market.PromotionAmount + (m.market == null
                                        ? 0
                                        : m.market.UpgradeAmount + m.market.SeasonAmount + m.market.BirthdayAmount)),
                        BetAmount = markets.Sum(m => m.market?.BetAmount ?? 0M),
                        ValidBetAmount = markets.Sum(m => m.market?.OriginalTurnover ?? 0M),
                        Turnover = markets.Sum(m => m.market?.Turnover ?? 0M),
                        WinLossAmount = markets.Sum(m => m.market?.WinLossAmount ?? 0M),
                        NetProfit = markets.Sum(m => m.market?.NetProfit ?? 0M),
                        AdditionalExpenses = markets.Sum(m => m.market?.AllAgentRebateAmount ?? 0M),
                        PlatformCoveredBonus = markets.Sum(m => m.market?.PlatformCoveredBonus ?? 0M),
                        PlatformCoveredBonusAdjust = markets.Sum(m => m.market?.PlatformCoveredBonusAdjust ?? 0M)
                    })
            .AsQueryable()
            .CreatePagedResponse(request);
    }
}