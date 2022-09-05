using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Enum;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player
{
    public class GetPlayerContributionsTotalAmountQueryHandler
        : IRequestHandler<GetPlayerContributionsTotalAmountRequest, PlayerContributionTotalAmountModel>
    {
        private readonly TgsContext _context;

        public GetPlayerContributionsTotalAmountQueryHandler(TgsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PlayerContributionTotalAmountModel> Handle(
            GetPlayerContributionsTotalAmountRequest request,
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

            var totalWithdrawal = to > today
                ? await _context.WithdrawForms
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
                    .Where(x => x.BrandId == request.BrandId)
                    .Where(x => x.UserType == 1 && x.Status == 9000)
                    .Where(x => x.CreatedOn >= today.ToUniversalTime() &&
                        x.CreatedOn < today.AddDays(1).ToUniversalTime())
                    .Where(!string.IsNullOrEmpty(request.Username), x => x.Username == request.Username)
                    .Where(request.HasUpperAgent == true, x => x.AgentId != null)
                    .Where(request.HasUpperAgent == false, x => x.AgentId == null)
                    .Where(request.Level, x => x.Level == request.Level)
                    .SumAsync(x => x.ActualAmount ?? x.Amount, cancellationToken)
                    .ConfigureAwait(false)
                : 0M;

            var totalDeposit = to > today
                ? await _context.DepositForms
                    .Join(_context.Players,
                        wf => wf.PlayerCode,
                        p => p.PlayerCode,
                        (wf, p) => new
                        {
                            wf.BrandId,
                            wf.Status,
                            wf.CreatedOn,
                            wf.PlayerCode,
                            wf.ActualAmount,
                            wf.Amount,
                            p.AgentId,
                            p.Username,
                            Level = p.Vip == null ? (int?)null : p.Vip.Level
                        })
                    .Where(x => x.BrandId == request.BrandId)
                    .Where(x => x.Status == 8000)
                    .Where(x => x.CreatedOn >= today.ToUniversalTime() &&
                        x.CreatedOn < today.AddDays(1).ToUniversalTime())
                    .Where(!string.IsNullOrEmpty(request.Username), x => x.Username == request.Username)
                    .Where(request.HasUpperAgent == true, x => x.AgentId != null)
                    .Where(request.HasUpperAgent == false, x => x.AgentId == null)
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
                .Where(request.HasUpperAgent == true, x => x.AgentId != null)
                .Where(request.HasUpperAgent == false, x => x.AgentId == null)
                .Where(request.Level, x => x.Level == request.Level)
                .GroupBy(x => true)
                .Select(group=> new
                {
                    DailyDepositAmount = group.Sum(s => s.DailyDepositAmount),
                    DailyWithdrawAmount = group.Sum(s => s.DailyWithdrawAmount),
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            return await _context.PlayerAggregates
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
                .Where(request.HasUpperAgent == true, x => x.AgentId != null)
                .Where(request.HasUpperAgent == false, x => x.AgentId == null)
                .Select(x => x.PlayerCode).Union(_context.PlayerBonusAggregates
                    .Where(x => x.BrandId == request.BrandId)
                    .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
                    .Where(request.HasUpperAgent == true, x => x.AgentId != null)
                    .Where(request.HasUpperAgent == false, x => x.AgentId == null)
                    .Select(x => x.PlayerCode))
                .GroupBy(x => x, (x, y) => new { PlayerCode = x })
                .GroupJoin(_context.PlayerAggregates
                    .Where(x => x.BrandId == request.BrandId)
                    .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
                    .Where(request.HasUpperAgent == true, x => x.AgentId != null)
                    .Where(request.HasUpperAgent == false, x => x.AgentId == null)
                    .GroupBy(x => x.PlayerCode, (playerCode, list) => new
                    {
                        PlayerCode = playerCode,
                        DepositAmount = list.Sum(p => p.DepositAmount),
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
                    }),
                    player => player.PlayerCode, aggregate => aggregate.PlayerCode,
                    (player, list) => new
                    {
                        player.PlayerCode,
                        list
                    })
                .SelectMany(x => x.list.DefaultIfEmpty(),
                    (group, playerAggregate) => new
                    {
                        group.PlayerCode,
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
                        bonusList
                    })
                .SelectMany(x => x.bonusList.DefaultIfEmpty(),
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
                .GroupBy(x => x.PlayerCode,
                    (playerCode, playerAggregates) => new
                    {
                        PlayerCode = playerCode,
                        DepositAmount = playerAggregates.Sum(p => p.DepositAmount),
                        CompanyAdjustAmount =
                            playerAggregates.Sum(p => p.CompanyAdjustAmount),
                        BonusAdjustAmount =
                            playerAggregates.Sum(p => p.BonusAdjustAmount),
                        AgentAdjustAmount =
                            playerAggregates.Sum(p => p.AgentAdjustAmount),
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
                .Join(_context.Players.Include(x => x.Vip).Select(x => new
                {
                    x.BrandId,
                    x.PlayerCode,
                    x.Username,
                    Level = x.Vip == null ? (int?)null : x.Vip.Level
                }), agg => agg.PlayerCode, p => p.PlayerCode, (agg, player) => new
                {
                    Username = player.Username,
                    PlayerCode = agg.PlayerCode,
                    Level = player.Level,
                    Type = AccountType.Player,
                    DepositAmount = agg.DepositAmount,
                    CompanyAdjustAmount = agg.CompanyAdjustAmount,
                    AgentAdjustAmount = agg.AgentAdjustAmount,
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
                .Select(x => new PlayerContributionTotalAmountModel
                {
                    DepositAmount = (dailyDepositWithdraws == null ? 0M : dailyDepositWithdraws.DailyDepositAmount ?? 0M) + totalDeposit,
                    WithdrawalAmount = (dailyDepositWithdraws == null ? 0M : dailyDepositWithdraws.DailyWithdrawAmount ?? 0M) + totalWithdrawal,
                    CompanyAdjustAmount = x.Sum(p => p.CompanyAdjustAmount),
                    BonusAdjustAmount = x.Sum(p => p.BonusAdjustAmount),
                    AgentAdjustAmount = x.Sum(p => p.AgentAdjustAmount),
                    BonusAmount = x.Sum(p => p.BonusAmount + p.UpgradeAmount + p.SeasonAmount + p.BirthdayAmount),
                    BetAmount = x.Sum(p => p.BetAmount),
                    ValidBetAmount = x.Sum(p => p.ValidBetAmount),
                    Turnover = x.Sum(p => p.Turnover),
                    WinLossAmount = x.Sum(p => p.WinLossAmount),
                    NetProfit = x.Sum(p => p.NetProfit - (p.UpgradeAmount + p.SeasonAmount + p.BirthdayAmount))
                }).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) ?? new PlayerContributionTotalAmountModel();
        }
    }
}