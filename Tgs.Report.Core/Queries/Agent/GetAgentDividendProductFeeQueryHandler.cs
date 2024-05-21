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
                    x.productFees
                        .Sum(s => s.CardGameWinLossAmount <= 0M ? 0M : s.CardGameWinLossAmount * s.CardGamePercentage / 100),
                LiveDealerAmount =
                    x.productFees
                        .Sum(s => s.LiveDealerWinLossAmount <= 0M ? 0M : s.LiveDealerWinLossAmount * s.LiveDealerPercentage / 100),
                LotteryAmount =
                    x.productFees
                        .Sum(s => s.LotteryWinLossAmount <= 0M ? 0M : s.LotteryWinLossAmount * s.LotteryPercentage / 100),
                SportsAmount =
                    x.productFees
                        .Sum(s => s.SportsWinLossAmount <= 0M ? 0M : s.SportsWinLossAmount * s.SportsPercentage / 100),
                SlotAmount =
                    x.productFees
                        .Sum(s => s.SlotWinLossAmount <= 0M ? 0M : s.SlotWinLossAmount * s.SlotPercentage / 100),
                VirtualSportsAmount =
                    x.productFees
                        .Sum(s => s.VirtualSportsWinLossAmount <= 0M ? 0M : s.VirtualSportsWinLossAmount * s.VirtualSportsPercentage / 100),
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