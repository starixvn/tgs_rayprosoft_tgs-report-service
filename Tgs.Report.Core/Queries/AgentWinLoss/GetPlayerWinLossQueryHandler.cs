using MediatR;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Infrastructures.Extensions.Models;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core.Queries.AgentWinLoss;

internal class GetPlayerWinLossQueryHandler(TgsContext context) : IRequestHandler<GetPlayerWinLossRequest,
    PagedResponse<AgentWinLossModel>>
{
    public Task<PagedResponse<AgentWinLossModel>> Handle(
        GetPlayerWinLossRequest request,
        CancellationToken cancellationToken)
    {
        return context.PlayerAggregates.Where(x => x.ReportDate >= request.From && x.ReportDate < request.To)
            .Where(x => x.PlayerCode == request.PlayerCode)

            .Where(x => x.AgentId != null)
            .GroupBy(x => x.PlayerCode, (playerCode, list) => new
            {
                PlayerCode = playerCode,
                RoundConut = list.Sum(s => s.RoundCount),
                Turnover = list.Sum(s => s.Turnover),
                WinLoss = list.Sum(s => s.WinLossAmount),
                AgentWinLoss = list.Sum(s => s.WinLossAmount * s.AgentDividendPercentage * 0.01M) ?? 0M,
                CompanyWinLoss = list.Sum(s => s.WinLossAmount * (100M - s.AgentDividendPercentage) * 0.01M) ?? 0M,
                Special =
                    list.Sum(s => s.RebateAmount + s.PromotionAmount + s.UpgradeAmount + s.SeasonAmount +
                        s.BirthdayAmount +
                        (s.BonusAdjustAmount - s.NoIncludeNetProfitBonusAdjustAmount)),
                AgentSpecial =
                    list.Sum(s => (s.RebateAmount + s.PromotionAmount + s.UpgradeAmount + s.SeasonAmount +
                        s.BirthdayAmount + (s.BonusAdjustAmount - s.NoIncludeNetProfitBonusAdjustAmount)) * s.AgentDividendPercentage * 0.01M) ?? 0M,
                CompanySpecial = list.Sum(s
                        => (s.RebateAmount + s.PromotionAmount + s.UpgradeAmount + s.SeasonAmount + s.BirthdayAmount +
                            (s.BonusAdjustAmount - s.NoIncludeNetProfitBonusAdjustAmount)) * (100M - s.AgentDividendPercentage) *
                        0.01M) ??
                    0M
            })
            .Join(context.Players, aggregate => aggregate.PlayerCode, p => p.PlayerCode,
                (agg, player) => new AgentWinLossModel
                {
                    Id = agg.PlayerCode,
                    Username = player.Username,
                    MemberType = "player",
                    RoundCount = agg.RoundConut,
                    Turnover = agg.Turnover,
                    WinLoss = agg.WinLoss,
                    AgentWinLoss = agg.AgentWinLoss,
                    CompanyWinLoss = agg.CompanyWinLoss,
                    Special = -agg.Special,
                    AgentSpecial = -agg.AgentSpecial,
                    CompanySpecial = -agg.CompanySpecial
                })
            .CreatePagedResponseAsync(request);
    }
}
