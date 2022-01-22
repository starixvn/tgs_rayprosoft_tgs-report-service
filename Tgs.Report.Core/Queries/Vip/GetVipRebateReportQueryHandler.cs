using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Vip;

namespace Tgs.Report.Core.Queries.Vip
{
    internal class
        GetVipRebateReportQueryHandler : IRequestHandler<GetVipRebateReportRequest, PagedResponse<VipRebateModel>>
    {
        private readonly ITgsRepository _repository;

        public GetVipRebateReportQueryHandler(ITgsRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResponse<VipRebateModel>> Handle(
            GetVipRebateReportRequest request,
            CancellationToken cancellationToken)
        {
            var offset = new TimeSpan(8, 0, 0);
            var requestFrom = request.From.ToOffset(offset);
            var requestTo = request.To.ToOffset(offset);
            DateTimeOffset from = new(requestFrom.Year, requestFrom.Month, requestFrom.Day, 0, 0, 0,
                requestFrom.Offset);
            DateTimeOffset to = new(requestTo.Year, requestTo.Month, requestTo.Day, 0, 0, 0,
                requestTo.Offset);

            var tgsPlayer =
                await _repository.Players.FirstOrDefaultAsync(x =>
                    x.BrandId == request.BrandId && x.Username == request.Username, cancellationToken);

            return await _repository.PlayerGameRebateAggregates
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.ReportDate >= from && x.ReportDate < to)
                .Where(tgsPlayer != null, x => x.PlayerCode == tgsPlayer.PlayerCode)
                .GroupBy(
                    x => new
                    {
                        x.ReportDate,
                        LiveDealerRebatePercentage = x.VipLiveDealerRebatePercentage,
                        CardGameRebatePercentage = x.VipCardGameRebatePercentage,
                        LotteryRebatePercentage = x.VipLotteryRebatePercentage,
                        SlotRebatePercentage = x.VipSlotRebatePercentage,
                        SportsRebatePercentage = x.VipSportsRebatePercentage,
                        VirtualSportsRebatePercentage = x.VipVirtualSportsRebatePercentage
                    }, (x, players) =>
                        new VipRebateModel
                        {
                            ReportDate = x.ReportDate,
                            LiveDealerRebatePercentage = x.LiveDealerRebatePercentage,
                            SlotRebatePercentage = x.SlotRebatePercentage,
                            LotteryRebatePercentage = x.LotteryRebatePercentage,
                            CardGameRebatePercentage = x.CardGameRebatePercentage,
                            SportsRebatePercentage = x.SportsRebatePercentage,
                            VirtualSportsRebatePercentage = x.VirtualSportsRebatePercentage,
                            LiveDealerAmount =
                                x.LiveDealerRebatePercentage * players
                                    .Where(l => l.GameType == (short)GameType.LiveDealer)
                                    .Sum(a => a.Turnover) / 100M,
                            LiveDealerTurnover =
                                players.Where(l => l.GameType == (short)GameType.LiveDealer).Sum(a => a.Turnover),
                            SlotAmount = x.SlotRebatePercentage * players.Where(l =>
                                        Consts.SlotGroup.Select(s => (short)s).Contains(l.GameType))
                                    .Sum(a => a.Turnover) /
                                100M,
                            SlotTurnover = players.Where(l =>
                                Consts.SlotGroup.Select(s => (short)s).Contains(l.GameType)).Sum(a => a.Turnover),
                            LotteryAmount = x.LotteryRebatePercentage * players
                                .Where(a => a.GameType == (short)GameType.Lottery)
                                .Sum(a => a.Turnover) / 100M,
                            LotteryTurnover = players.Where(a => a.GameType == (short)GameType.Lottery)
                                .Sum(a => a.Turnover),
                            CardGameAmount =
                                x.CardGameRebatePercentage *
                                players.Where(l => l.GameType == (short)GameType.CardGame).Sum(a => a.Turnover) / 100M,
                            CardGameTurnover = players.Where(l => l.GameType == (short)GameType.CardGame)
                                .Sum(a => a.Turnover),
                            SportsAmount = x.SportsRebatePercentage * players.Where(l =>
                                    Consts.SportsGroup.Select(s => (short)s).Contains(l.GameType))
                                .Sum(x => x.Turnover) / 100M,
                            SportsTurnover = players.Where(l =>
                                    Consts.SportsGroup.Select(s => (short)s).Contains(l.GameType))
                                .Sum(x => x.Turnover),
                            VirtualSportsAmount =
                                x.VirtualSportsRebatePercentage *
                                players.Where(l => l.GameType == (short)GameType.VirtualSports).Sum(a => a.Turnover) /
                                100M,
                            VirtualSportsTurnover = players.Where(l => l.GameType == (short)GameType.VirtualSports)
                                .Sum(a => a.Turnover)
                        })
                .CreatePagedResponseAsync(request);
        }
    }
}