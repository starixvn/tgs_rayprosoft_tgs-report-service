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

            var totalWithdrawal = await _context.WithdrawForms
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
                .Where(x => x.CreatedOn >= from.ToUniversalTime() && x.CreatedOn < to.ToUniversalTime())
                .Where(!string.IsNullOrEmpty(request.Username), x => x.Username == request.Username)
                .Where(request.HasUpperAgent == true, x => x.AgentId != null)
                .Where(request.HasUpperAgent == false, x => x.AgentId == null)
                .Where(request.Level, x => x.Level == request.Level)
                .SumAsync(x => x.ActualAmount ?? x.Amount, cancellationToken)
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
                    .Where(request.HasUpperAgent == false, x => x.AgentId == null),
                    player => player.PlayerCode, aggregate => aggregate.PlayerCode,
                    (player, list) => new
                    {
                        player.PlayerCode,
                        list
                    })
                .SelectMany(x => x.list.DefaultIfEmpty(),
                    (group, aggegate) => new
                    {
                        PlayerCode = group.PlayerCode,
                        aggegate
                    })
                .GroupJoin(_context.PlayerBonusAggregates
                        .Where(x => x.BrandId == request.BrandId)
                        .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
                        .Where(request.HasUpperAgent == true, x => x.AgentId != null)
                        .Where(request.HasUpperAgent == false, x => x.AgentId == null),
                    player => player.PlayerCode, aggregate => aggregate.PlayerCode,
                    (player, bonusList) => new
                    {
                        player.PlayerCode,
                        player.aggegate,
                        bonusList
                    })
                .SelectMany(x => x.bonusList.DefaultIfEmpty(),
                    (player, bonus) => new
                    {
                        player.PlayerCode,
                        player.aggegate,
                        bonus
                    })
                .GroupBy(x => x.PlayerCode,
                    (playerCode, playerAggregates) => new
                    {
                        PlayerCode = playerCode,
                        DepositAmount = playerAggregates.Sum(p => p.aggegate == null ? 0M : p.aggegate.DepositAmount),
                        CompanyAdjustAmount =
                            playerAggregates.Sum(p => p.aggegate == null ? 0M : p.aggegate.CompanyAdjustAmount - p.aggegate.NoIncludeNetProfitCompanyAdjustAmount),
                        BonusAdjustAmount =
                            playerAggregates.Sum(p => p.aggegate == null ? 0M : p.aggegate.BonusAdjustAmount - p.aggegate.NoIncludeNetProfitBonusAdjustAmount),
                        AgentAdjustAmount =
                            playerAggregates.Sum(p => p.aggegate == null ? 0M : p.aggegate.AgentAdjustAmount - p.aggegate.NoIncludeNetProfitAgentAdjustAmount),
                        BonusAmount = playerAggregates.Sum(p => (p.aggegate == null ? 0M : p.aggegate.RebateAmount + p.aggegate.PromotionAmount) +
                            (p.bonus == null
                                ? 0M
                                : p.bonus.UpgradeAmount + p.bonus.SeasonAmount + p.bonus.BirthdayAmount)),
                        BetAmount = playerAggregates.Sum(p => p.aggegate == null ? 0M : p.aggegate.BetAmount),
                        ValidBetAmount = playerAggregates.Sum(p => p.aggegate == null ? 0M : p.aggegate.OriginalTurnover),
                        Turnover = playerAggregates.Sum(p => p.aggegate == null ? 0M : p.aggegate.Turnover),
                        WinLossAmount = playerAggregates.Sum(p => p.aggegate == null ? 0M : p.aggegate.WinLossAmount),
                        NetProfit = playerAggregates.Sum(p
                            => (p.aggegate == null ? 0M : p.aggegate.WinLossAmount - (p.aggegate.AgentAdjustAmount - p.aggegate.NoIncludeNetProfitAgentAdjustAmount) -
                            (p.aggegate.BonusAdjustAmount - p.aggegate.NoIncludeNetProfitBonusAdjustAmount) -
                            p.aggegate.RebateAmount - p.aggegate.PromotionAmount) -
                            (p.bonus == null
                                ? 0M
                                : p.bonus.UpgradeAmount + p.bonus.SeasonAmount + p.bonus.BirthdayAmount))
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
                    NetProfit = agg.NetProfit
                })
                .Where(!string.IsNullOrEmpty(request.Username), x => x.Username == request.Username)
                .Where(request.Level, x => x.Level == request.Level)
                .GroupBy(x => true)
                .Select(x => new PlayerContributionTotalAmountModel
                {
                    DepositAmount = x.Sum(p => p.DepositAmount),
                    WithdrawalAmount = totalWithdrawal,
                    CompanyAdjustAmount = x.Sum(p => p.CompanyAdjustAmount),
                    BonusAdjustAmount = x.Sum(p => p.BonusAdjustAmount),
                    AgentAdjustAmount = x.Sum(p => p.AgentAdjustAmount),
                    BonusAmount = x.Sum(p => p.BonusAmount),
                    BetAmount = x.Sum(p => p.BetAmount),
                    ValidBetAmount = x.Sum(p => p.ValidBetAmount),
                    Turnover = x.Sum(p => p.Turnover),
                    WinLossAmount = x.Sum(p => p.WinLossAmount),
                    NetProfit = x.Sum(p => p.NetProfit)
                }).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) ?? new PlayerContributionTotalAmountModel();
        }
    }
}