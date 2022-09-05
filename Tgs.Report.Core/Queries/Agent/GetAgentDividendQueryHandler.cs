using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent
{
    public class GetAgentDividendQueryHandler : IRequestHandler<GetAgentDividendsRequest, PagedResponse<AgentDividendModel>>
    {
        private readonly TgsContext _context;

        public GetAgentDividendQueryHandler(TgsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PagedResponse<AgentDividendModel>> Handle(
            GetAgentDividendsRequest request,
            CancellationToken cancellationToken)
        {
            var mainAgent = await _context.Agents.Where(x => x.BrandId == request.BrandId)
                .Where(request.Username, x => request.Username == x.Username)
                .Where(request.AgentId, x => x.Id == request.AgentId)
                .Select(x => new
                {
                    x.Id,
                    x.Username
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (mainAgent is null)
            {
                return new PagedResponse<AgentDividendModel>
                {
                    Valid = false,
                    Message = Resources.Agent.NotFound
                };
            }

            var offset = new TimeSpan(8, 0, 0);
            var requestFrom = request.From.ToOffset(offset);
            var requestTo = request.To.ToOffset(offset);

            var from = new DateTimeOffset(requestFrom.Year,
                requestFrom.Month,
                requestFrom.Day,
                0,
                0,
                0,
                requestFrom.Offset);

            var to = new DateTimeOffset(requestTo.Year,
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
                offset);

            var fromDate = new DateOnly(from.Year, from.Month, from.Day);
            var toDate = new DateOnly(to.Year, to.Month, to.Day);

            var dailyDepositWithdraws = await _context.PlayerDailyPaymentStatistics
                .Join(_context.Players.Where(x => x.BrandId == request.BrandId), pp => pp.PlayerCode, p => p.PlayerCode,(pp,p)=>new
                {
                    pp.ReportDate,
                    p.AgentId,
                    pp.DailyDepositAmount,
                    pp.DailyWithdrawAmount
                })
                .Where(x => x.ReportDate >= fromDate && x.ReportDate < toDate)
                .GroupBy(x => x.AgentId,(agentId,group)=> new
                {
                    AgentId = agentId,
                    DailyDepositAmount = group.Sum(s => s.DailyDepositAmount),
                    DailyWithdrawAmount = group.Sum(s => s.DailyWithdrawAmount),
                })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            var subAgents = await _context.Agents.Where(x => x.ParentAgentId == mainAgent.Id)
                .Select(x => new
                {
                    x.Id,
                    x.Username
                })
                .ToListAsync(cancellationToken);

            var agents = subAgents.Union(new[]{mainAgent});

            var agentIds = agents.Select(x => x.Id).ToArray();

            var agentDividends = await _context.AgentDividends
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.From >= request.From.ToUniversalTime())
                .Where(x => x.To < request.To.ToUniversalTime())
                .Where(x => agentIds.Contains(x.AgentId))
                .Select(x => new
                {
                    x.AgentId,
                    x.DepositAmount,
                    x.WithdrawalAmount,
                    x.CompanyAdjustAmount,
                    x.AgentAdjustAmount,
                    x.BonusAmount,
                    x.BetAmount,
                    x.ValidBetAmount,
                    x.OriginalTurnover,
                    x.Turnover,
                    x.WinLossAmount,
                    x.NetProfit,
                    x.AgentDividendAmount,
                    x.BeforeDividendAmount,
                    x.NoIncludeNetProfitAgentAdjustAmount,
                    x.ReferencedAgentDividendId,
                    x.SentOn,
                    x.AgentDividendMultipleLayers,
                    x.BonusAdjustAmount,
                    x.NoIncludeNetProfitBonusAdjustAmount
                }).ToArrayAsync(cancellationToken);

            var agentDividendGroup = agentDividends.GroupBy(x => x.AgentId,
                (agentId, list) => new
                {
                    AgentId = agentId,
                    CompanyAdjustAmount = list.Sum(l => l.CompanyAdjustAmount),
                    AgentAdjustAmount = list.Sum(l => l.AgentAdjustAmount),
                    BonusAdjustAmount = list.Sum(l => l.BonusAdjustAmount),
                    BonusAmount = list.Sum(l => l.BonusAmount),
                    BetAmount = list.Sum(l => l.BetAmount),
                    ValidBetAmount = list.Sum(l => l.ValidBetAmount),
                    OriginalTurnover = list.Sum(l => l.OriginalTurnover),
                    Turnover = list.Sum(l => l.Turnover),
                    WinLossAmount = list.Sum(l => l.WinLossAmount),
                    NetProfit = list.Sum(l => l.NetProfit),
                    AgentDividendAmount =
                        list.Sum(l => l.BeforeDividendAmount ?? l.AgentDividendAmount),
                    NoIncludeNetProfitAgentAdjustAmount = list.Sum(l => l.NoIncludeNetProfitAgentAdjustAmount),
                    NoIncludeNetProfitBonusAdjustAmount = list.Sum(l => l.NoIncludeNetProfitBonusAdjustAmount),
                    FromPlayerAgentDividendAmount = list.Sum(l => l.AgentDividendMultipleLayers.Where(m => m.AgentId == m.DividendProviderAgentId).Sum(s => s.Dividend)),
                    FromTeamAgentDividendAmount = list.Sum(l => l.AgentDividendMultipleLayers.Where(m => m.AgentId != m.DividendProviderAgentId).Sum(s => s.Dividend)),
                }).ToArray();

            var models = agentDividendGroup.Join(agents.ToArray(),
                ad => ad.AgentId,
                a => a.Id,
                (ad, a) => new AgentDividendModel
                {
                    Id = ad.AgentId,
                    Username = a.Username,
                    DepositAmount = dailyDepositWithdraws.FirstOrDefault(a => a.AgentId == ad.AgentId)?.DailyDepositAmount ?? 0M,
                    WithdrawalAmount = dailyDepositWithdraws.FirstOrDefault(a => a.AgentId == ad.AgentId)?.DailyWithdrawAmount ?? 0M,
                    CompanyAdjustAmount = ad.CompanyAdjustAmount,
                    AgentAdjustAmount = ad.AgentAdjustAmount - ad.NoIncludeNetProfitAgentAdjustAmount,
                    BonusAdjustAmount = ad.BonusAdjustAmount - ad.NoIncludeNetProfitBonusAdjustAmount,
                    BonusAmount = ad.BonusAmount,
                    BetAmount = ad.BetAmount,
                    ValidBetAmount = ad.OriginalTurnover,
                    Turnover = ad.Turnover,
                    WinLossAmount = ad.WinLossAmount,
                    NetProfit = ad.NetProfit,
                    AgentDividendAmount = ad.AgentDividendAmount,
                    FromPlayerAgentDividendAmount = ad.FromPlayerAgentDividendAmount,
                    FromTeamAgentDividendAmount = ad.FromTeamAgentDividendAmount,
                }).ToArray();

            return models.AsQueryable().CreatePagedResponse(request);
        }
    }
}