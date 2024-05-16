using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent;

public class
    GetAgentDividendProductFeeQueryHandler(
        ITgsService<AgentDividendProductFee> agentDividendProductFeeService,
        ITgsService<AgentDividend> agentDividendService)
    : IRequestHandler<GetAgentDividendProductFeeRequest, AgentDividendProductFeeModel>
{
    public async Task<AgentDividendProductFeeModel> Handle(
        GetAgentDividendProductFeeRequest request,
        CancellationToken cancellationToken)
    {
        var fee = await agentDividendService.GetQueries()
            .Where(x => x.AgentId == request.AgentId)
            .Where(x => x.AgentDividendPercentage == request.AgentDividendPercentage)
            .Where(x => x.From >= request.From.ToUniversalTime())
            .Where(x => x.To < request.To.ToUniversalTime())
            .GroupJoin(agentDividendProductFeeService.GetQueries(),
                a => a.Id,
                a => a.AgentDividendId,
                (agentDividend, productFees) => new { agentDividend, productFees })
            .Select(x => new
            {
                x.agentDividend.AgentId,
                CardGameAmount =
                    x.productFees.Where(w => w.GameType == (short)GameType.CardGame)
                        .Sum(s => (s.WinLossAmount < 0 ? 0 : s.WinLossAmount) * s.Percentage / 100),
                LiveDealerAmount =
                    x.productFees.Where(w => w.GameType == (short)GameType.LiveDealer)
                        .Sum(s => (s.WinLossAmount < 0 ? 0 : s.WinLossAmount) * s.Percentage / 100),
                LotteryAmount =
                    x.productFees.Where(w => w.GameType == (short)GameType.Lottery)
                        .Sum(s => (s.WinLossAmount < 0 ? 0 : s.WinLossAmount) * s.Percentage / 100),
                SportsAmount =
                    x.productFees.Where(w => Consts.SportsGroup.Select(s => (short)s).Contains(w.GameType))
                        .Sum(s => (s.WinLossAmount < 0 ? 0 : s.WinLossAmount) * s.Percentage / 100),
                SlotAmount =
                    x.productFees.Where(w => Consts.SlotGroup.Select(s => (short)s).Contains(w.GameType))
                        .Sum(s => (s.WinLossAmount < 0 ? 0 : s.WinLossAmount) * s.Percentage / 100),
                VirtualSportsAmount = x.productFees.Where(w => w.GameType == (short)GameType.VirtualSports)
                    .Sum(s => (s.WinLossAmount < 0 ? 0 : s.WinLossAmount) * s.Percentage / 100),
            })
            .GroupBy(x => x.AgentId)
            .Select(x => new AgentDividendProductFeeModel
            {
                CardGameAmount = x.Sum(s => s.CardGameAmount),
                LiveDealerAmount = x.Sum(s => s.LiveDealerAmount),
                LotteryAmount = x.Sum(s => s.LotteryAmount),
                SportsAmount = x.Sum(s => s.SportsAmount),
                SlotAmount = x.Sum(s => s.SlotAmount),
                VirtualSportsAmount = x.Sum(s => s.VirtualSportsAmount),
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return fee ?? new AgentDividendProductFeeModel();
    }
}