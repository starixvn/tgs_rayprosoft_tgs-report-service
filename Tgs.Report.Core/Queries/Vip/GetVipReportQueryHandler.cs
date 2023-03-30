using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Vip;

namespace Tgs.Report.Core.Queries.Vip;

public class GetVipReportQueryHandler : IRequestHandler<GetVipReportRequest, PagedResponse<VipModel>>
{
    private readonly TgsContext _context;

    public GetVipReportQueryHandler(TgsContext context) => _context = context;

    public async Task<PagedResponse<VipModel>> Handle(
        GetVipReportRequest request,
        CancellationToken cancellationToken)
    {
        var offset = new TimeSpan(8, 0, 0);
        var requestFrom = request.From.ToOffset(offset);
        var requestTo = request.To.ToOffset(offset);

        DateTimeOffset from = new(requestFrom.Year,
            requestFrom.Month,
            requestFrom.Day,
            0,
            0,
            0,
            requestFrom.Offset);

        DateTimeOffset to = new(requestTo.Year,
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

        var fromDate = new DateOnly(from.Year, from.Month, from.Day);
        var toDate = new DateOnly(to.Year, to.Month, to.Day);

        var withdrawalByUserIds = to > today
            ? await _context.WithdrawForms
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.UserType == 1 && x.Status == 9000)
                .Where(x => x.CreatedOn >= from.ToUniversalTime() && x.CreatedOn < to.ToUniversalTime())
                .Join(_context.Players,
                    wf => wf.UserId,
                    p => p.PlayerCode,
                    (wf, p) => new
                    {
                        wf.BrandId,
                        wf.UserType,
                        wf.Status,
                        wf.CreatedOn,
                        wf.UserId,
                        wf.ActualAmount,
                        wf.Amount,
                        p.AgentId,
                        p.Username,
                        Level = p.Vip == null ? (int?)null : p.Vip.Level
                    })
                .Where(!string.IsNullOrEmpty(request.Username), x => x.Username == request.Username)
                .Where(request.Level, x => x.Level == request.Level)
                .GroupBy(x => x.UserId,
                    (userId, withdrawals) => new
                    {
                        UserId = userId,
                        WithdrawalAmount = withdrawals.Sum(w => w.ActualAmount ?? w.Amount)
                    }).ToArrayAsync(cancellationToken)
                .ConfigureAwait(false)
            : null;

        var depositByUserIds = to > today
            ? await _context.DepositForms
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.Status == 8000)
                .Where(x => x.CreatedOn >= from.ToUniversalTime() && x.CreatedOn < to.ToUniversalTime())
                .Join(_context.Players,
                    df => df.PlayerCode,
                    p => p.PlayerCode,
                    (df, p) => new
                    {
                        df.BrandId,
                        df.Status,
                        df.CreatedOn,
                        df.PlayerCode,
                        df.ActualAmount,
                        df.Amount,
                        p.AgentId,
                        p.Username,
                        Level = p.Vip == null ? (int?)null : p.Vip.Level
                    })
                .Where(!string.IsNullOrEmpty(request.Username), x => x.Username == request.Username)
                .Where(request.Level, x => x.Level == request.Level)
                .GroupBy(x => x.PlayerCode,
                    (playerCode, deposits) => new
                    {
                        PlayerCode = playerCode,
                        DepositAmount = deposits.Sum(w => w.ActualAmount ?? w.Amount)
                    }).ToArrayAsync(cancellationToken)
                .ConfigureAwait(false)
            : null;

        var dailyDepositWithdraws = await _context.PlayerDailyPaymentStatistics
            .Join(_context.Players.Where(x => x.BrandId == request.BrandId), pp => pp.PlayerCode, p => p.PlayerCode,
                (pp, p) => new
                {
                    pp.ReportDate,
                    p.PlayerCode,
                    pp.DailyDepositAmount,
                    pp.DailyWithdrawAmount
                })
            .Where(x => x.ReportDate >= fromDate && x.ReportDate < toDate)
            .GroupBy(x => x.PlayerCode, (playerCode, group) => new
            {
                PlayerCode = playerCode,
                DailyDepositAmount = group.Sum(s => s.DailyDepositAmount),
                DailyWithdrawAmount = group.Sum(s => s.DailyWithdrawAmount)
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var vips = await _context.PlayerAggregates
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
            .Where(x => x.CreatedOn >= from.ToUniversalTime())
            .Select(x => x.PlayerCode).Union(_context.PlayerBonusAggregates
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
                .Select(x => x.PlayerCode))
            .GroupBy(x => x, (x, y) => new { PlayerCode = x })
            .GroupJoin(_context.PlayerAggregates
                    .Where(x => x.BrandId == request.BrandId)
                    .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
                    .Where(x => x.CreatedOn >= from.ToUniversalTime())
                    .GroupBy(x => new
                        {
                            x.PlayerCode,
                            x.BrandId
                        },
                        (x, players) => new
                        {
                            x.BrandId,
                            x.PlayerCode,
                            DepositAmount = players.Sum(m => m.DepositAmount),
                            WithdrawalAmount = players.Sum(m => m.WithdrawalAmount),
                            CompanyAdjustAmount =
                                players.Sum(m => m.CompanyAdjustAmount - m.NoIncludeNetProfitCompanyAdjustAmount),
                            BonusAdjustAmount =
                                players.Sum(m => m.BonusAdjustAmount - m.NoIncludeNetProfitBonusAdjustAmount),
                            BonusAmount = players.Sum(m => m.RebateAmount + m.PromotionAmount),
                            BetAmount = players.Sum(m => m.BetAmount),
                            ValidBetAmount = players.Sum(m => m.OriginalTurnover),
                            Turnover = players.Sum(m => m.Turnover),
                            WinLossAmount = players.Sum(m => m.WinLossAmount),
                            NetProfit = players.Sum(m
                                => m.WinLossAmount - (m.CompanyAdjustAmount - m.NoIncludeNetProfitCompanyAdjustAmount) -
                                m.RebateAmount - m.PromotionAmount)
                        })
                    .GroupJoin(_context.PlayerGameRebateAggregates
                            .Where(x => x.BrandId == request.BrandId)
                            .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime()),
                        p => new
                        {
                            p.BrandId,
                            p.PlayerCode
                        },
                        pg => new
                        {
                            pg.BrandId,
                            pg.PlayerCode
                        },
                        (p, pg) => new
                        {
                            p,
                            pg
                        })
                    .SelectMany(x => x.pg.DefaultIfEmpty(),
                        (x, pg) => new
                        {
                            x.p.BrandId,
                            x.p.PlayerCode,
                            x.p.DepositAmount,
                            x.p.WithdrawalAmount,
                            x.p.CompanyAdjustAmount,
                            x.p.BonusAdjustAmount,
                            x.p.BonusAmount,
                            x.p.BetAmount,
                            x.p.ValidBetAmount,
                            x.p.Turnover,
                            x.p.WinLossAmount,
                            x.p.NetProfit,
                            pg
                        })
                    .GroupBy(x => new
                        {
                            x.BrandId,
                            x.PlayerCode,
                            x.DepositAmount,
                            x.WithdrawalAmount,
                            x.CompanyAdjustAmount,
                            x.BonusAdjustAmount,
                            x.BonusAmount,
                            x.BetAmount,
                            x.ValidBetAmount,
                            x.Turnover,
                            x.WinLossAmount,
                            x.NetProfit
                        },
                        (x, players) => new
                        {
                            x.BrandId,
                            x.PlayerCode,
                            x.DepositAmount,
                            x.WithdrawalAmount,
                            x.CompanyAdjustAmount,
                            x.BonusAdjustAmount,
                            RebateAmount =
                                players.Sum(m =>
                                    m.pg == null ? 0M : m.pg.VipRebateAmount),
                            x.BonusAmount,
                            x.BetAmount,
                            x.ValidBetAmount,
                            x.Turnover,
                            x.WinLossAmount,
                            x.NetProfit
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
                    playerAggregate
                })
            .GroupJoin(_context.PlayerBonusAggregates
                    .Where(x => x.BrandId == request.BrandId)
                    .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
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
                    player.playerAggregate,
                    markets
                })
            .SelectMany(x => x.markets.DefaultIfEmpty(),
                (x, bonus) => new
                {
                    x.PlayerCode,
                    x.playerAggregate,
                    bonus
                })
            .GroupBy(x => x.PlayerCode, (playerCode, group) => new
            {
                PlayerCode = playerCode,
                DepositAmount = group.Sum(s => s.playerAggregate.DepositAmount),
                CompanyAdjustAmount = group.Sum(s => s.playerAggregate.CompanyAdjustAmount),
                BonusAdjustAmount = group.Sum(s => s.playerAggregate.BonusAdjustAmount),
                RebateAmount = group.Sum(s => s.playerAggregate.RebateAmount),
                BonusAmount = group.Sum(s => s.playerAggregate.BonusAmount),
                BetAmount = group.Sum(s => s.playerAggregate.BetAmount),
                ValidBetAmount = group.Sum(s => s.playerAggregate.ValidBetAmount),
                Turnover = group.Sum(s => s.playerAggregate.Turnover),
                WinLossAmount = group.Sum(s => s.playerAggregate.WinLossAmount),
                NetProfit = group.Sum(s => s.playerAggregate.NetProfit),
                UpgradeAmount = group.Sum(p => p.bonus.UpgradeAmount),
                SeasonAmount = group.Sum(p => p.bonus.SeasonAmount),
                BirthdayAmount = group.Sum(p => p.bonus.BirthdayAmount)
            })
            .Join(_context.Players.Include(x => x.Vip).Select(x => new
            {
                x.BrandId,
                x.PlayerCode,
                x.Username,
                Level = x.Vip == null ? (int?)null : x.Vip.Level
            }), agg => agg.PlayerCode, p => p.PlayerCode, (agg, player) => new VipModel
            {
                Username = player.Username,
                Level = player.Level,
                BrandId = player.BrandId,
                PlayerCode = agg.PlayerCode,
                DepositAmount = agg.DepositAmount,
                CompanyAdjustAmount = agg.CompanyAdjustAmount,
                BonusAdjustAmount = agg.BonusAdjustAmount,
                RebateAmount = agg.RebateAmount,
                BonusAmount = agg.BonusAmount + agg.UpgradeAmount + agg.SeasonAmount + agg.BirthdayAmount,
                BetAmount = agg.BetAmount,
                ValidBetAmount = agg.ValidBetAmount,
                Turnover = agg.Turnover,
                WinLossAmount = agg.WinLossAmount,
                NetProfit = agg.NetProfit - (agg.UpgradeAmount + agg.SeasonAmount + agg.BirthdayAmount)
            })
            .Where(!string.IsNullOrEmpty(request.Username), x => x.Username == request.Username)
            .Where(request.Level, x => x.Level == request.Level)
            .CreatePagedResponseAsync(request);

        if (vips.Items == null)
        {
            return new()
            {
                Valid = true,
                Items = Array.Empty<VipModel>()
            };
        }

        foreach (var item in vips.Items)
        {
            item.DepositAmount = (dailyDepositWithdraws.FirstOrDefault(x => x.PlayerCode == item.PlayerCode)
                    ?.DailyDepositAmount ?? 0M)
                + (depositByUserIds?.FirstOrDefault(x => x.PlayerCode == item.PlayerCode)?.DepositAmount ?? 0M);

            item.WithdrawalAmount = (dailyDepositWithdraws.FirstOrDefault(x => x.PlayerCode == item.PlayerCode)
                    ?.DailyWithdrawAmount ?? 0M)
                + (withdrawalByUserIds?.FirstOrDefault(w => w.UserId == item.PlayerCode)?.WithdrawalAmount ?? 0);
        }

        return vips;
    }
}