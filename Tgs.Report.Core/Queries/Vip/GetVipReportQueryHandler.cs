using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Vip;

namespace Tgs.Report.Core.Queries.Vip
{
    public class GetVipReportQueryHandler : IRequestHandler<GetVipReportRequest, PagedResponse<VipModel>>
    {
        private readonly ITgsRepository _repository;

        public GetVipReportQueryHandler(ITgsRepository repository)
        {
            _repository = repository;
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

            var brandPlayers = await _repository.Players.Where(x => x.BrandId == request.BrandId)
                .Include(x => x.Vip)
                .ToArrayAsync(cancellationToken);

            var tgsPlayer = !string.IsNullOrEmpty(request.Username)
                ? brandPlayers.FirstOrDefault(x =>
                    x.BrandId == request.BrandId && x.Username == request.Username)
                : null;

            var tgsPlayers = request.Level == null
                ? null
                : brandPlayers.Where(x => x.Vip?.Level == request.Level)
                    .Select(x => x.PlayerCode).ToArray();

            var withdrawalByUserIds = await _repository.WithdrawForms
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.UserType == 1 && x.Status == 9000)
                .Where(x => x.CreatedOn >= from && x.CreatedOn < to)
                .GroupBy(x => x.UserId,
                    (userId, withdrawals) => new
                    {
                        UserId = userId,
                        WithdrawalAmount = withdrawals.Sum(w => w.ActualAmount ?? w.Amount)
                    }).ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            var vips = await _repository.PlayerAggregates
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.ReportDate >= from && x.ReportDate < to)
                .Where(tgsPlayer != null, x => x.PlayerCode == tgsPlayer.PlayerCode)
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
                        AdjustmentAmount = players.Sum(m => m.AdjustmentAmount),
                        BonusAmount = players.Sum(m => m.RebateAmount + m.PromotionAmount),
                        BetAmount = players.Sum(m => m.BetAmount),
                        ValidBetAmount = players.Sum(m => m.OriginalTurnover),
                        Turnover = players.Sum(m => m.Turnover),
                        WinLossAmount = players.Sum(m => m.WinLossAmount),
                        NetProfit = players.Sum(m
                            => m.WinLossAmount - m.AdjustmentAmount - m.RebateAmount - m.PromotionAmount)
                    })
                .GroupJoin(_repository.PlayerGameRebateAggregates
                        .Where(x => x.BrandId == request.BrandId)
                        .Where(x => x.ReportDate >= from && x.ReportDate < to)
                        .Where(tgsPlayer != null, x => x.PlayerCode == tgsPlayer.PlayerCode),
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
                        x.p.AdjustmentAmount,
                        x.p.BonusAmount,
                        x.p.BetAmount,
                        x.p.ValidBetAmount,
                        x.p.Turnover,
                        x.p.WinLossAmount,
                        x.p.NetProfit,
                        pg
                    })
                .Where(request.Level, x => tgsPlayers.Contains(x.PlayerCode))
                .GroupBy(x => new
                    {
                        x.BrandId,
                        x.PlayerCode,
                        x.DepositAmount,
                        x.WithdrawalAmount,
                        x.AdjustmentAmount,
                        x.BonusAmount,
                        x.BetAmount,
                        x.ValidBetAmount,
                        x.Turnover,
                        x.WinLossAmount,
                        x.NetProfit
                    },
                    (x, players) => new VipModel
                    {
                        BrandId = x.BrandId,
                        PlayerCode = x.PlayerCode,
                        DepositAmount = x.DepositAmount,
                        WithdrawalAmount = x.WithdrawalAmount,
                        AdjustmentAmount = x.AdjustmentAmount,
                        RebateAmount =
                            players.Sum(m =>
                                m.pg.VipRebateAmount),
                        BonusAmount = x.BonusAmount,
                        BetAmount = x.BetAmount,
                        ValidBetAmount = x.ValidBetAmount,
                        Turnover = x.Turnover,
                        WinLossAmount = x.WinLossAmount,
                        NetProfit = x.NetProfit
                    }).ToArrayAsync(cancellationToken);

            return vips.Select(x =>
            {
                var player = brandPlayers.FirstOrDefault(p => p.PlayerCode == x.PlayerCode);

                return new VipModel
                {
                    Username = player?.Username,
                    Level = player?.Vip?.Level,
                    BrandId = x.BrandId,
                    PlayerCode = x.PlayerCode,
                    DepositAmount = x.DepositAmount,
                    WithdrawalAmount =
                        withdrawalByUserIds.FirstOrDefault(w => w.UserId == player.PlayerCode)?.WithdrawalAmount ?? 0,
                    AdjustmentAmount = x.AdjustmentAmount,
                    RebateAmount = x.RebateAmount,
                    BonusAmount = x.BonusAmount,
                    BetAmount = x.BetAmount,
                    ValidBetAmount = x.ValidBetAmount,
                    Turnover = x.Turnover,
                    WinLossAmount = x.WinLossAmount,
                    NetProfit = x.NetProfit
                };
            }).AsQueryable().CreatePagedResponse(request);
        }
    }
}