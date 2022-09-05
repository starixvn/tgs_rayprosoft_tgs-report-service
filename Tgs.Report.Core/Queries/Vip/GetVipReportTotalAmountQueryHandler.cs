using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Vip;

namespace Tgs.Report.Core.Queries.Vip;

public class GetVipReportTotalAmountQueryHandler : IRequestHandler<GetVipReportTotalAmountRequest, VipTotalAmountModel>
{
    private readonly TgsContext _context;

    public GetVipReportTotalAmountQueryHandler(TgsContext context)
    {
        _context = context;
    }

    public async Task<VipTotalAmountModel> Handle(
        GetVipReportTotalAmountRequest request,
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

        var totalWithdrawal = to > today ? await _context.WithdrawForms
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.UserType == 1 && x.Status == 9000)
            .Where(x => x.CreatedOn >= from.ToUniversalTime() && x.CreatedOn < to.ToUniversalTime())
            .Join(_context.Players,
                wf => wf.UserId,
                p => p.PlayerCode,
                (wf, p) => new
                {
                    wf.UserId,
                    wf.ActualAmount,
                    wf.Amount,
                    p.Username,
                    Level = p.Vip == null ? (int?)null : p.Vip.Level
                })
            .Where(!string.IsNullOrEmpty(request.Username), x => x.Username == request.Username)
            .Where(request.Level, x => x.Level == request.Level)
            .SumAsync(x => x.ActualAmount ?? x.Amount, cancellationToken)
            .ConfigureAwait(false)
            : 0M;

        var totalDeposit = to > today ? await _context.DepositForms
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.Status == 8000)
                .Where(x => x.CreatedOn >= from.ToUniversalTime() && x.CreatedOn < to.ToUniversalTime())
                .Join(_context.Players,
                    wf => wf.PlayerCode,
                    p => p.PlayerCode,
                    (df, p) => new
                    {
                        df.PlayerCode,
                        df.ActualAmount,
                        df.Amount,
                        p.Username,
                        Level = p.Vip == null ? (int?)null : p.Vip.Level
                    })
                .Where(!string.IsNullOrEmpty(request.Username), x => x.Username == request.Username)
                .Where(request.Level, x => x.Level == request.Level)
                .SumAsync(x => x.ActualAmount ?? x.Amount, cancellationToken)
                .ConfigureAwait(false)
            : 0M;

        var dailyDepositWithdraws = await _context.PlayerDailyPaymentStatistics
            .Join(_context.Players.Where(x => x.BrandId == request.BrandId), pp => pp.PlayerCode, p => p.PlayerCode,(pp,p)=>new
            {
                pp.ReportDate,
                p.AgentId,
                p.PlayerCode,
                p.Username,
                Level = p.Vip == null ? (int?)null : p.Vip.Level,
                pp.DailyDepositAmount,
                pp.DailyWithdrawAmount
            })
            .Where(x => x.ReportDate >= fromDate && x.ReportDate < toDate)
            .Where(!string.IsNullOrEmpty(request.Username), x => x.Username == request.Username)
            .Where(request.Level, x => x.Level == request.Level)
            .GroupBy(x => true)
            .Select(group=> new
            {
                DailyDepositAmount = group.Sum(s => s.DailyDepositAmount),
                DailyWithdrawAmount = group.Sum(s => s.DailyWithdrawAmount),
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var vipTotalAmount = await _context.PlayerAggregates
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
            .Select(x => x.PlayerCode).Union(_context.PlayerBonusAggregates
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
                .Select(x => x.PlayerCode))
            .GroupBy(x => x, (x, y) => new { PlayerCode = x })
            .GroupJoin(_context.PlayerAggregates
                    .Where(x => x.BrandId == request.BrandId)
                    .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
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
                        ValidBetAmount = list.Sum(p => p.OriginalTurnover),
                        Turnover = list.Sum(p => p.Turnover),
                        WinLossAmount = list.Sum(p => p.WinLossAmount),
                        NetProfit = list.Sum(p => p.WinLossAmount - (p.CompanyAdjustAmount - p.NoIncludeNetProfitCompanyAdjustAmount) -
                            p.RebateAmount - p.PromotionAmount)
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
                    playerAggregate.DepositAmount,
                    playerAggregate.CompanyAdjustAmount,
                    playerAggregate.AgentAdjustAmount,
                    playerAggregate.BonusAdjustAmount,
                    playerAggregate.BonusAmount,
                    playerAggregate.BetAmount,
                    playerAggregate.ValidBetAmount,
                    playerAggregate.Turnover,
                    playerAggregate.WinLossAmount,
                    playerAggregate.NetProfit
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
                    bonus.UpgradeAmount,
                    bonus.SeasonAmount,
                    bonus.BirthdayAmount
                })
            .GroupBy(x => x.PlayerCode, (playerCode, group) => new
            {
                PlayerCode = playerCode,
                DepositAmount = group.Sum(s => s.DepositAmount),
                CompanyAdjustAmount = group.Sum(s => s.CompanyAdjustAmount),
                BonusAdjustAmount = group.Sum(s => s.BonusAdjustAmount),
                RebateAmount = group.Sum(s => s.BonusAdjustAmount),
                BonusAmount = group.Sum(s => s.BonusAmount),
                BetAmount = group.Sum(s => s.BetAmount),
                ValidBetAmount = group.Sum(s => s.ValidBetAmount),
                Turnover = group.Sum(s => s.Turnover),
                WinLossAmount = group.Sum(s => s.WinLossAmount),
                NetProfit = group.Sum(s => s.NetProfit),
                UpgradeAmount = group.Sum(s => s.UpgradeAmount),
                SeasonAmount = group.Sum(s => s.SeasonAmount),
                BirthdayAmount = group.Sum(s => s.BirthdayAmount)
            })
            .Join(_context.Players.Include(x => x.Vip).Select(x => new
            {
                x.BrandId,
                x.PlayerCode,
                x.Username,
                Level = x.Vip == null ? (int?)null : x.Vip.Level
            }), agg => agg.PlayerCode, p => p.PlayerCode, (agg, player) => new
            {
                Username = player.Username,
                Level = player.Level,
                BrandId = player.BrandId,
                PlayerCode = agg.PlayerCode,
                DepositAmount = agg.DepositAmount,
                CompanyAdjustAmount = agg.CompanyAdjustAmount,
                BonusAdjustAmount = agg.BonusAdjustAmount,
                BonusAmount = agg.BonusAmount,
                BetAmount = agg.BetAmount,
                ValidBetAmount = agg.ValidBetAmount,
                Turnover = agg.Turnover,
                WinLossAmount = agg.WinLossAmount,
                NetProfit = agg.NetProfit,
                agg.UpgradeAmount,
                agg.SeasonAmount,
                agg.BirthdayAmount
            })
            .Where(!string.IsNullOrEmpty(request.Username), x => x.Username == request.Username)
            .Where(request.Level, x => x.Level == request.Level)
            .GroupBy(x => true)
            .Select(x => new
            {
                DepositAmount = x.Sum(s => s.DepositAmount),
                CompanyAdjustAmount = x.Sum(s => s.CompanyAdjustAmount),
                BonusAdjustAmount = x.Sum(s => s.BonusAdjustAmount),
                BonusAmount = x.Sum(s => s.BonusAmount),
                BetAmount = x.Sum(s => s.BetAmount + s.UpgradeAmount + s.SeasonAmount + s.BirthdayAmount),
                ValidBetAmount = x.Sum(s => s.ValidBetAmount),
                Turnover = x.Sum(s => s.Turnover),
                WinLossAmount = x.Sum(s => s.WinLossAmount),
                NetProfit = x.Sum(s => s.NetProfit - (s.UpgradeAmount + s.SeasonAmount + s.BirthdayAmount))
            })
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);


            var vipRebateAmount = await _context.PlayerGameRebateAggregates
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
                .GroupBy(x => new
                    {
                        x.PlayerCode,
                        x.BrandId
                    },
                    (x, players) => new
                    {
                        x.BrandId,
                        x.PlayerCode,
                        RebateAmount = players.Sum(m => m.VipRebateAmount)
                    })
                .Join(_context.Players.Include(x => x.Vip).Select(x => new
                {
                    x.BrandId,
                    x.PlayerCode,
                    x.Username,
                    Level = x.Vip == null ? (int?)null : x.Vip.Level
                }), agg => agg.PlayerCode, p => p.PlayerCode, (agg, player) => new
                {
                    Username = player.Username,
                    Level = player.Level,
                    BrandId = agg.BrandId,
                    PlayerCode = agg.PlayerCode,
                    RebateAmount = agg.RebateAmount
                })
                .Where(!string.IsNullOrEmpty(request.Username), x => x.Username == request.Username)
                .Where(request.Level, x => x.Level == request.Level)
                .SumAsync(x => x.RebateAmount, cancellationToken)
                .ConfigureAwait(false);

            return new VipTotalAmountModel
            {
                DepositAmount = dailyDepositWithdraws?.DailyDepositAmount ?? 0M + totalDeposit,
                WithdrawalAmount = dailyDepositWithdraws?.DailyWithdrawAmount ?? 0M + totalWithdrawal,
                CompanyAdjustAmount = vipTotalAmount?.CompanyAdjustAmount ?? 0M,
                BonusAdjustAmount = vipTotalAmount?.BonusAdjustAmount ?? 0M,
                BonusAmount = vipTotalAmount?.BonusAmount ?? 0M,
                BetAmount = vipTotalAmount?.BetAmount ?? 0M,
                ValidBetAmount = vipTotalAmount?.ValidBetAmount ?? 0M,
                Turnover = vipTotalAmount?.Turnover ?? 0M,
                WinLossAmount = vipTotalAmount?.WinLossAmount ?? 0M,
                NetProfit = vipTotalAmount?.NetProfit ?? 0M,
                RebateAmount = vipRebateAmount
            };
    }
}