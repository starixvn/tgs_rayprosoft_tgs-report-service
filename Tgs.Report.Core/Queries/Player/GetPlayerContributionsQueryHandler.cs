using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player
{
    public class
        GetPlayerContributionsQueryHandler : IRequestHandler<GetPlayerContributionsRequest,
            PagedResponse<PlayerContributionModel>>
    {
        private readonly ITgsRepository _repository;

        public GetPlayerContributionsQueryHandler(ITgsRepository repository)
        {
            _repository = repository;
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

            var players = request.HasUpperAgent is null
                ? await _repository.Players.Where(x => x.BrandId == request.BrandId)
                    .Include(x => x.Vip)
                    .ToArrayAsync(cancellationToken)
                : await _repository.Players.Where(x => x.BrandId == request.BrandId && x.AgentId.HasValue == request.HasUpperAgent)
                    .Include(x => x.Vip)
                    .ToArrayAsync(cancellationToken);

            var tgsPlayer = !string.IsNullOrEmpty(request.Username)
                ? players.FirstOrDefault(x =>
                    x.BrandId == request.BrandId && x.Username == request.Username)
                : null;

            if (!string.IsNullOrEmpty(request.Username) && tgsPlayer is null)
            {
                return new PagedResponse<PlayerContributionModel>
                    { Valid = false, Message = Resources.Player.NotFound };
            }

            var tgsPlayers = request.Level == null
                ? null
                : players.Where(x => x.Vip?.Level == request.Level)
                    .Select(x => x.PlayerCode).ToArray();

            var withdrawalByUserIds = await _repository.WithdrawForms
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.UserType == 1 && x.Status == 9000)
                .Where(x => x.CreatedOn >= from.ToUniversalTime() && x.CreatedOn < to.ToUniversalTime())
                .Where(!string.IsNullOrEmpty(request.Username), x => x.UserId == tgsPlayer.PlayerCode)
                .Where(request.Level, x => tgsPlayers.Contains(x.UserId))
                .GroupBy(x => x.UserId,
                    (userId, withdrawals) => new
                    {
                        UserId = userId,
                        WithdrawalAmount = withdrawals.Sum(w => w.ActualAmount ?? w.Amount)
                    }).ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            var reportPlayers = await _repository.PlayerAggregates
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
                .Where(!string.IsNullOrEmpty(request.Username), x => x.PlayerCode == tgsPlayer.PlayerCode)
                .Where(request.Level, x => tgsPlayers.Contains(x.PlayerCode))
                .GroupBy(x => x.PlayerCode,
                    (playerCode, players) => new
                    {
                        PlayerCode = playerCode,
                        DepositAmount = players.Sum(p => p.DepositAmount),
                        AdjustmentAmount = players.Sum(p => p.AdjustmentAmount),
                        BonusAmount = players.Sum(p => p.RebateAmount + p.PromotionAmount),
                        BetAmount = players.Sum(p => p.BetAmount),
                        ValidBetAmount = players.Sum(p => p.OriginalTurnover),
                        Turnover = players.Sum(p => p.Turnover),
                        WinLossAmount = players.Sum(p => p.WinLossAmount),
                        NetProfit = players.Sum(p
                            => p.WinLossAmount - p.AdjustmentAmount - p.RebateAmount - p.PromotionAmount)
                    })
                .ToArrayAsync(cancellationToken);

            var outOfReportPlayers = withdrawalByUserIds.Select(x => x.UserId)
                .Except(reportPlayers.Select(x => x.PlayerCode)).ToArray();

            return reportPlayers.Select(x =>
                {
                    var player = players.FirstOrDefault(p => p.PlayerCode == x.PlayerCode);

                    return new PlayerContributionModel
                    {
                        Username = player?.Username,
                        PlayerCode = x.PlayerCode,
                        Level = player?.Vip?.Level,
                        Type = AccountType.Player,
                        DepositAmount = x.DepositAmount,
                        WithdrawalAmount =
                            withdrawalByUserIds.FirstOrDefault(w => w.UserId == x.PlayerCode)?.WithdrawalAmount ?? 0,
                        AdjustmentAmount = x.AdjustmentAmount,
                        BonusAmount = x.BonusAmount,
                        BetAmount = x.BetAmount,
                        ValidBetAmount = x.ValidBetAmount,
                        Turnover = x.Turnover,
                        WinLossAmount = x.WinLossAmount,
                        NetProfit = x.NetProfit
                    };
                }).Union(outOfReportPlayers.Select(playerCode =>
                {
                    var player = players.FirstOrDefault(p => p.PlayerCode == playerCode);

                    return new PlayerContributionModel
                    {
                        Username = player?.Username,
                        PlayerCode = playerCode,
                        Level = player?.Vip?.Level,
                        Type = AccountType.Player,
                        DepositAmount = 0,
                        WithdrawalAmount =
                            withdrawalByUserIds.FirstOrDefault(w => w.UserId == playerCode)?.WithdrawalAmount ?? 0,
                        AdjustmentAmount = 0,
                        BonusAmount = 0,
                        BetAmount = 0,
                        ValidBetAmount = 0,
                        Turnover = 0,
                        WinLossAmount = 0,
                        NetProfit = 0
                    };
                }))
                .AsQueryable().CreatePagedResponse(request);
        }
    }
}