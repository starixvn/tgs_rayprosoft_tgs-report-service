using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Enum;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player;

public class GetPlayerContributionsQueryHandler(TgsContext context)
    : IRequestHandler<GetPlayerContributionsRequest, PagedResponse<PlayerContributionModel>>
{
    private readonly TgsContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<PagedResponse<PlayerContributionModel>> Handle(
        GetPlayerContributionsRequest request,
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
        var yesterday = new DateTimeOffset(now.Year,
            now.Month,
            now.Day,
            0,
            0,
            0,
            requestTo.Offset).AddDays(-1);

        var fromDate = new DateOnly(from.Year, from.Month, from.Day);
        var toDate = to > yesterday
            ? new(yesterday.Year, yesterday.Month, yesterday.Day)
            : new DateOnly(to.Year, to.Month, to.Day);

        var withdrawalByUserIds = to > yesterday
            ? await _context.WithdrawForms
                .Join(_context.Players,
                    wf => wf.UserId,
                    p => p.PlayerCode,
                    (wf, p) => new
                    {
                        wf.BrandId,
                        wf.UserType,
                        wf.Status,
                        wf.PaidOn,
                        wf.UserId,
                        wf.ActualAmount,
                        wf.Amount,
                        p.AgentId,
                        p.Username,
                        Level = p.Vip == null ? (int?)null : p.Vip.Level
                    })
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.UserType == 1 && x.Status == 9000)
                .Where(from > yesterday, x => x.PaidOn >= from.UtcDateTime && x.PaidOn < to.UtcDateTime)
                .Where(!(from > yesterday), x => x.PaidOn >= yesterday.UtcDateTime && x.PaidOn < to.UtcDateTime)
                .Where(!string.IsNullOrEmpty(request.Username), x => x.Username == request.Username)
                .Where(request.HasUpperAgent == true, x => x.AgentId != null)
                .Where(request.HasUpperAgent == false, x => x.AgentId == null)
                .Where(request.Level, x => x.Level == request.Level)
                .GroupBy(x => x.UserId,
                    (userId, withdrawals) => new
                    {
                        UserId = userId,
                        WithdrawalAmount = withdrawals.Sum(w => w.ActualAmount ?? w.Amount)
                    }).ToArrayAsync(cancellationToken)
                .ConfigureAwait(false)
            : null;

        var depositByUserIds = to > yesterday
            ? await _context.DepositForms
                .Join(_context.Players,
                    df => df.PlayerCode,
                    p => p.PlayerCode,
                    (df, p) => new
                    {
                        df.BrandId,
                        df.Status,
                        df.UpdatedOn,
                        df.PlayerCode,
                        df.ActualAmount,
                        df.Amount,
                        df.HandlingFee,
                        p.AgentId,
                        p.Username,
                        Level = p.Vip == null ? (int?)null : p.Vip.Level
                    })
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.Status == 8000)
                .Where(from > yesterday, x => x.UpdatedOn >= from.UtcDateTime && x.UpdatedOn < to.UtcDateTime)
                .Where(!(from > yesterday), x => x.UpdatedOn >= yesterday.UtcDateTime && x.UpdatedOn < to.UtcDateTime)
                .Where(!string.IsNullOrEmpty(request.Username), x => x.Username == request.Username)
                .Where(request.HasUpperAgent == true, x => x.AgentId != null)
                .Where(request.HasUpperAgent == false, x => x.AgentId == null)
                .Where(request.Level, x => x.Level == request.Level)
                .GroupBy(x => x.PlayerCode,
                    (userId, deposits) => new
                    {
                        UserId = userId,
                        DepositAmount = deposits.Sum(w => w.ActualAmount ?? w.Amount),
                        DepositHandlingFee = deposits.Sum(w => w.HandlingFee)
                    }).ToArrayAsync(cancellationToken)
                .ConfigureAwait(false)
            : null;

        var playerDailyPaymentStatistics = await _context.PlayerDailyPaymentStatistics
            .Join(_context.Players.Where(x => x.BrandId == request.BrandId), pp => pp.PlayerCode, p => p.PlayerCode,
                (pp, p) => new
                {
                    pp.ReportDate,
                    p.AgentId,
                    p.PlayerCode,
                    p.Username,
                    Level = p.Vip == null ? (int?)null : p.Vip.Level,
                    pp.DailyDepositAmount,
                    pp.DailyDepositHandlingFee,
                    pp.DailyWithdrawAmount
                })
            .Where(x => x.ReportDate >= fromDate && x.ReportDate < toDate)
            .Where(!string.IsNullOrEmpty(request.Username), x => x.Username == request.Username)
            .Where(request.HasUpperAgent == true, x => x.AgentId != null)
            .Where(request.HasUpperAgent == false, x => x.AgentId == null)
            .Where(request.Level, x => x.Level == request.Level)
            .GroupBy(x => x.PlayerCode, (playerCode, group) => new
            {
                PlayerCode = playerCode,
                DailyDepositAmount = group.Sum(s => s.DailyDepositAmount),
                DailyDepositHandlingFee = group.Sum(s => s.DailyDepositHandlingFee),
                DailyWithdrawAmount = group.Sum(s => s.DailyWithdrawAmount)
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var playerCodes = playerDailyPaymentStatistics.Select(p => p.PlayerCode)
            .Concat(depositByUserIds?.Select(p => p.UserId) ?? Array.Empty<string>())
            .Concat(withdrawalByUserIds?.Select(p => p.UserId) ?? Array.Empty<string>());

        var playerContributions = await _context.PlayerAggregates
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate >= from.UtcDateTime && x.ReportDate < to.UtcDateTime)
            .Where(request.HasUpperAgent == true, x => x.AgentId != null)
            .Where(request.HasUpperAgent == false, x => x.AgentId == null)
            .Where(x => x.CreatedOn >= from.UtcDateTime)
            .GroupBy(x => x.PlayerCode, (playerCode, list) => new
            {
                PlayerCode = playerCode,
                CompanyAdjustAmount =
                    list.Sum(p => p.CompanyAdjustAmount - p.NoIncludeNetProfitCompanyAdjustAmount),
                AgentAdjustAmount = list.Sum(p => p.AgentAdjustAmount - p.NoIncludeNetProfitAgentAdjustAmount),
                BonusAdjustAmount = list.Sum(p => p.BonusAdjustAmount - p.NoIncludeNetProfitBonusAdjustAmount),
                BonusAmount = list.Sum(p => p.RebateAmount + p.PromotionAmount),
                BetAmount = list.Sum(p => p.BetAmount),
                ValidBetAmount = list.Sum(p => p.OriginalTurnover),
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
            .GroupJoin(_context.PlayerBonusAggregates
                    .Where(x => x.BrandId == request.BrandId)
                    .Where(x => x.ReportDate >= from.UtcDateTime && x.ReportDate < to.UtcDateTime)
                    .Where(request.HasUpperAgent == true, x => x.AgentId != null)
                    .Where(request.HasUpperAgent == false, x => x.AgentId == null)
                    .GroupBy(x => x.PlayerCode, (playerCode, list) => new
                    {
                        PlayerCode = playerCode,
                        UpgradeAmount = list.Sum(p => p.UpgradeAmount),
                        SeasonAmount = list.Sum(p => p.SeasonAmount),
                        BirthdayAmount = list.Sum(p => p.BirthdayAmount)
                    }),
                player => player.PlayerCode, aggregate => aggregate.PlayerCode,
                (player, bonusList) => new
                {
                    player.PlayerCode,
                    player.CompanyAdjustAmount,
                    player.AgentAdjustAmount,
                    player.BonusAdjustAmount,
                    player.BonusAmount,
                    player.BetAmount,
                    player.ValidBetAmount,
                    player.Turnover,
                    player.WinLossAmount,
                    player.NetProfit,
                    bonusList
                })
            .SelectMany(x => x.bonusList.DefaultIfEmpty(),
                (x, bonus) => new
                {
                    x.PlayerCode,
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
            .GroupBy(x => x.PlayerCode,
                (playerCode, playerAggregates) => new
                {
                    PlayerCode = playerCode,
                    CompanyAdjustAmount = playerAggregates.Sum(p => p.CompanyAdjustAmount),
                    BonusAdjustAmount = playerAggregates.Sum(p => p.BonusAdjustAmount),
                    AgentAdjustAmount = playerAggregates.Sum(p => p.AgentAdjustAmount),
                    BonusAmount = playerAggregates.Sum(p => p.BonusAmount),
                    BetAmount = playerAggregates.Sum(p => p.BetAmount),
                    ValidBetAmount = playerAggregates.Sum(p => p.ValidBetAmount),
                    Turnover = playerAggregates.Sum(p => p.Turnover),
                    WinLossAmount = playerAggregates.Sum(p => p.WinLossAmount),
                    NetProfit = playerAggregates.Sum(p => p.NetProfit),
                    UpgradeAmount = playerAggregates.Sum(p => p.UpgradeAmount),
                    SeasonAmount = playerAggregates.Sum(p => p.SeasonAmount),
                    BirthdayAmount = playerAggregates.Sum(p => p.BirthdayAmount)
                })
            .Join(_context.Players.Include(x => x.Vip),
                aggregate => aggregate.PlayerCode,
                p => p.PlayerCode,
                (aggregate, p) => new
                {
                    Aggregate = aggregate,
                    p.Username,
                    Level = p.VipId == null ? (int?)null : p.Vip!.Level
                })
            .Where(!string.IsNullOrEmpty(request.Username), x => x.Username == request.Username)
            .Where(request.Level != null, x => x.Level == request.Level)
            .Select(x => new PlayerContributionModel
            {
                Username = x.Username,
                PlayerCode = x.Aggregate.PlayerCode,
                Level = x.Level,
                Type = AccountType.Player,
                CompanyAdjustAmount = x.Aggregate.CompanyAdjustAmount,
                AgentAdjustAmount = x.Aggregate.AgentAdjustAmount,
                BonusAdjustAmount = x.Aggregate.BonusAdjustAmount,
                BonusAmount =
                    x.Aggregate.BonusAmount + x.Aggregate.UpgradeAmount + x.Aggregate.SeasonAmount +
                    x.Aggregate.BirthdayAmount,
                BetAmount = x.Aggregate.BetAmount,
                ValidBetAmount = x.Aggregate.ValidBetAmount,
                Turnover = x.Aggregate.Turnover,
                WinLossAmount = x.Aggregate.WinLossAmount,
                NetProfit = x.Aggregate.NetProfit -
                    (x.Aggregate.UpgradeAmount + x.Aggregate.SeasonAmount + x.Aggregate.BirthdayAmount)
            })
            .Where(x =>
                playerCodes.Contains(x.PlayerCode)
                || x.CompanyAdjustAmount != 0
                || x.AgentAdjustAmount != 0
                || x.BonusAdjustAmount != 0
                || x.BonusAmount != 0
                || x.BetAmount != 0
                || x.ValidBetAmount != 0
                || x.Turnover != 0
                || x.WinLossAmount != 0
                || x.NetProfit != 0)
            .CreatePagedResponseAsync(request);

        if (playerContributions.Items != null)
        {
            foreach (var item in playerContributions.Items)
            {
                var statistics = playerDailyPaymentStatistics
                    .FirstOrDefault(x => x.PlayerCode == item.PlayerCode);

                var depositItem = depositByUserIds?
                    .FirstOrDefault(x => x.UserId == item.PlayerCode);

                var withdrawItem = withdrawalByUserIds?.FirstOrDefault(w => w.UserId == item.PlayerCode);

                item.DepositAmount = (statistics?.DailyDepositAmount ?? 0M) + (depositItem?.DepositAmount ?? 0M);

                item.DepositHandlingFee = (statistics?.DailyDepositHandlingFee ?? 0M) +
                    (depositItem?.DepositHandlingFee ?? 0M);

                item.WithdrawalAmount = (statistics?.DailyWithdrawAmount ?? 0M) + (withdrawItem?.WithdrawalAmount ?? 0);
            }
        }

        return playerContributions.Items == null
            ? new()
            {
                Valid = true,
                Items = Array.Empty<PlayerContributionModel>()
            }
            : playerContributions;
    }
}