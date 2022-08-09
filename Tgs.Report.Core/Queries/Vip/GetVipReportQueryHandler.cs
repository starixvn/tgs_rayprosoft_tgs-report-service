using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Vip;

namespace Tgs.Report.Core.Queries.Vip;

public class GetVipReportQueryHandler : IRequestHandler<GetVipReportRequest, PagedResponse<VipModel>>
{
    private readonly TgsContext _context;

    public GetVipReportQueryHandler(TgsContext context)
    {
        _context = context;
    }

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

        var withdrawalByUserIds = await _context.WithdrawForms
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
            .ConfigureAwait(false);

        var vips = await _context.PlayerAggregates
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
                    UpgradeAmount = bonus == null ? 0M : bonus.UpgradeAmount,
                    SeasonAmount = bonus == null ? 0M : bonus.SeasonAmount,
                    BirthdayAmount = bonus == null ? 0M : bonus.BirthdayAmount
                })
            .GroupJoin(_context.PlayerGameRebateAggregates
                    .Where(x => x.BrandId == request.BrandId)
                    .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
                    .GroupBy(x => x.PlayerCode,(playerCode, list) => new
                    {
                        PlayerCode = playerCode,
                        RebateAmount = list.Sum(s => s.VipRebateAmount)
                    }),
                p => p.PlayerCode,
                pg => pg.PlayerCode,
                (player, pg) => new
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
                    player.UpgradeAmount,
                    player.SeasonAmount,
                    player.BirthdayAmount,
                    pg
                })
            .SelectMany(x => x.pg.DefaultIfEmpty(),
                (x, pg) => new
                {
                    x.PlayerCode,
                    x.DepositAmount,
                    x.CompanyAdjustAmount,
                    x.BonusAdjustAmount,
                    x.BonusAmount,
                    x.BetAmount,
                    x.ValidBetAmount,
                    x.Turnover,
                    x.WinLossAmount,
                    x.NetProfit,
                    x.UpgradeAmount,
                    x.SeasonAmount,
                    x.BirthdayAmount,
                    pg.RebateAmount
                })
            .GroupBy(x => x.PlayerCode,(playerCode,group) => new
            {
                PlayerCode= playerCode,
                DepositAmount = group.Sum(s => s.DepositAmount),
                CompanyAdjustAmount = group.Sum(s => s.CompanyAdjustAmount),
                BonusAdjustAmount = group.Sum(s => s.BonusAdjustAmount),
                RebateAmount = group.Sum(s => s.BonusAdjustAmount),
                BonusAmount = group.Sum(s => s.BonusAmount + s.UpgradeAmount + s.SeasonAmount + s.BirthdayAmount),
                BetAmount = group.Sum(s => s.BetAmount),
                ValidBetAmount = group.Sum(s => s.ValidBetAmount),
                Turnover = group.Sum(s => s.Turnover),
                WinLossAmount = group.Sum(s => s.WinLossAmount),
                NetProfit = group.Sum(s => s.NetProfit - s.UpgradeAmount - s.SeasonAmount + s.BirthdayAmount)
                
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
                BonusAmount = agg.BonusAmount,
                BetAmount = agg.BetAmount,
                ValidBetAmount = agg.ValidBetAmount,
                Turnover = agg.Turnover,
                WinLossAmount = agg.WinLossAmount,
                NetProfit = agg.NetProfit
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
            item.WithdrawalAmount =
                withdrawalByUserIds.FirstOrDefault(w => w.UserId == item.PlayerCode)?.WithdrawalAmount ?? 0;

        return vips;
    }
}