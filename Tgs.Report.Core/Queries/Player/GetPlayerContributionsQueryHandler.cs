using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Enum;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player
{
    public class GetPlayerContributionsQueryHandler
        : IRequestHandler<GetPlayerContributionsRequest, PagedResponse<PlayerContributionModel>>
    {
        private readonly TgsContext _context;

        public GetPlayerContributionsQueryHandler(TgsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

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

            var withdrawalByUserIds = await _context.WithdrawForms
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
                        Level = p.VipId == null ? (int?)null : p.Vip.Level
                    })
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.UserType == 1 && x.Status == 9000)
                .Where(x => x.CreatedOn >= from.ToUniversalTime() && x.CreatedOn < to.ToUniversalTime())
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
                .ConfigureAwait(false);

            var reportPlayers = await _context.PlayerAggregates
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
                .Where(request.HasUpperAgent == true, x => x.AgentId != null)
                .Where(request.HasUpperAgent == false, x => x.AgentId == null)
                .GroupBy(x => x.PlayerCode,
                    (playerCode, playerAggregates) => new
                    {
                        PlayerCode = playerCode,
                        DepositAmount = playerAggregates.Sum(p => p.DepositAmount),
                        CompanyAdjustAmount =
                            playerAggregates.Sum(p => p.CompanyAdjustAmount - p.NoIncludeNetProfitCompanyAdjustAmount),
                        BonusAdjustAmount =
                            playerAggregates.Sum(p => p.BonusAdjustAmount - p.NoIncludeNetProfitBonusAdjustAmount),
                        AgentAdjustAmount =
                            playerAggregates.Sum(p => p.AgentAdjustAmount - p.NoIncludeNetProfitAgentAdjustAmount),
                        BonusAmount = playerAggregates.Sum(p => p.RebateAmount + p.PromotionAmount),
                        BetAmount = playerAggregates.Sum(p => p.BetAmount),
                        ValidBetAmount = playerAggregates.Sum(p => p.OriginalTurnover),
                        Turnover = playerAggregates.Sum(p => p.Turnover),
                        WinLossAmount = playerAggregates.Sum(p => p.WinLossAmount),
                        NetProfit = playerAggregates.Sum(p
                            => p.WinLossAmount - (p.AgentAdjustAmount - p.NoIncludeNetProfitAgentAdjustAmount) -
                            (p.BonusAdjustAmount - p.NoIncludeNetProfitBonusAdjustAmount) -
                            p.RebateAmount - p.PromotionAmount)
                    })
                .Join(_context.Players.Include(x => x.Vip).Select(x => new
                {
                    x.BrandId,
                    x.PlayerCode,
                    x.Username,
                    Level = x.VipId == null ? (int?)null : x.Vip.Level
                }), agg => agg.PlayerCode, p => p.PlayerCode, (agg, player) => new PlayerContributionModel
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
                .CreatePagedResponseAsync(request);

            if (reportPlayers.Items == null)
            {
                return new PagedResponse<PlayerContributionModel>
                {
                    Valid = true,
                    Items = Array.Empty<PlayerContributionModel>()
                };
            }

            foreach (var item in reportPlayers.Items)
            {
                item.WithdrawalAmount =
                    withdrawalByUserIds.FirstOrDefault(w => w.UserId == item.PlayerCode)?.WithdrawalAmount ?? 0;
            }

            return reportPlayers;
        }
    }
}