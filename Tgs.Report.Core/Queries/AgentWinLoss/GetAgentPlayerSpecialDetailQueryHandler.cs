using MediatR;
using Tgs.Report.Enum;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core.Queries.AgentWinLoss;

internal class GetAgentPlayerSpecialDetailQueryHandler(IAgentFactory agentFactory)
    : IRequestHandler<GetAgentPlayerSpecialDetailRequest,
        AgentPlayerSpecialDetailModel>
{
    public async Task<AgentPlayerSpecialDetailModel> Handle(
        GetAgentPlayerSpecialDetailRequest request,
        CancellationToken cancellationToken)
    {
        var response = await agentFactory.GetAgentPlayerSpecialFeesAsync(new GetAgentPlayerSpecialFeesDataRequest
        {
            BrandId = request.BrandId, AgentId = request.AgentId, From = request.From, To = request.To
        });

        var productFeeResponse = await agentFactory.GetAgentPlayerSpecialProductFeeAsync(
            new GetAgentPlayerSpecialProductFeeDataRequest
            {
                BrandId = request.BrandId, AgentId = request.AgentId, From = request.From, To = request.To
            }).ToArrayAsync(cancellationToken);

        var productFee = productFeeResponse.GroupBy(x => x.AgentId,
            (agentId, value) => new
            {
                AgentId = agentId,
                CardGameAmount =
                    value.Where(w => w.GameType == GameType.CardGame).Sum(s => s.WinLossAmount) < 0
                        ? 0
                        : value.Where(w => w.GameType == GameType.CardGame)
                            .Sum(s => s.WinLossAmount * s.ProduceFeePercentage / 100),
                LiveDealerAmount =
                    value.Where(w => w.GameType == GameType.LiveDealer).Sum(s => s.WinLossAmount) < 0
                        ? 0
                        : value.Where(w => w.GameType == GameType.LiveDealer)
                            .Sum(s => s.WinLossAmount * s.ProduceFeePercentage / 100),
                LotteryAmount =
                    value.Where(w => w.GameType == GameType.Lottery).Sum(s => s.WinLossAmount) < 0
                        ? 0
                        : value.Where(w => w.GameType == GameType.Lottery)
                            .Sum(s => s.WinLossAmount * s.ProduceFeePercentage / 100),
                SportsAmount =
                    value.Where(w => Consts.SportsGroup.Select(s => s).Contains(w.GameType)).Sum(s => s.WinLossAmount) <
                    0
                        ? 0
                        : value.Where(w => Consts.SportsGroup.Select(s => s).Contains(w.GameType))
                            .Sum(s => s.WinLossAmount * s.ProduceFeePercentage / 100),
                SlotAmount =
                    value.Where(w => Consts.SlotGroup.Select(s => s).Contains(w.GameType)).Sum(s => s.WinLossAmount) < 0
                        ? 0
                        : value.Where(w => Consts.SlotGroup.Select(s => s).Contains(w.GameType))
                            .Sum(s => s.WinLossAmount * s.ProduceFeePercentage / 100),
                VirtualSportsAmount =
                    value.Where(w => w.GameType == GameType.VirtualSports).Sum(s => s.WinLossAmount) < 0
                        ? 0
                        : value.Where(w => w.GameType == GameType.VirtualSports)
                            .Sum(s => s.WinLossAmount * s.ProduceFeePercentage / 100)
            });

        return new AgentPlayerSpecialDetailModel
        {
            Bonus = response.Bonus,
            BonusAdjust = response.BonusAdjust,
            PlatformCharge = response.PlatformCharge,
            ProductFee = productFee.Sum(s =>
                s.CardGameAmount + s.LiveDealerAmount + s.LotteryAmount + s.SlotAmount + s.SportsAmount +
                s.VirtualSportsAmount)
        };
    }
}
