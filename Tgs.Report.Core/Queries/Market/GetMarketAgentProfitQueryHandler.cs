using MediatR;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Market;

namespace Tgs.Report.Core.Queries.Market;

public class
    GetMarketAgentProfitQueryHandler : IRequestHandler<GetMarketAgentProfitsRequest,
        PagedResponse<MarketAgentProfitModel>>
{
    private readonly TgsContext _context;

    public GetMarketAgentProfitQueryHandler(TgsContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<PagedResponse<MarketAgentProfitModel>> Handle(
        GetMarketAgentProfitsRequest request,
        CancellationToken cancellationToken)
    {
        var offset = new TimeSpan(8, 0, 0);
        var requestFrom = request.Date.ToOffset(offset);

        var from = new DateTimeOffset(requestFrom.Year,
            requestFrom.Month,
            requestFrom.Day,
            0,
            0,
            0,
            requestFrom.Offset);

        var response = await _context.PlayerAggregates
            .GroupJoin(_context.PlayerBonusAggregates,
                agent => new
                {
                    agent.ReportDate,
                    agent.Hour,
                    agent.AgentId,
                    agent.PlayerCode
                },
                bonus => new
                {
                    bonus.ReportDate,
                    bonus.Hour,
                    bonus.AgentId,
                    bonus.PlayerCode
                },
                (agent, bonus) => new
                {
                    AgentAggregate = agent,
                    BonusAggregates = bonus
                })
            .SelectMany(x => x.BonusAggregates.DefaultIfEmpty(),
                (x, bonusAggregate) => new
                {
                    x.AgentAggregate,
                    BonusAggregate = bonusAggregate
                })
            .Where(x => x.AgentAggregate.BrandId == request.BrandId)
            .Where(x => x.AgentAggregate.ReportDate == from.ToUniversalTime())
            .Where(x => x.AgentAggregate.AgentId != null && x.AgentAggregate.AgentDividendPercentageTreeData != null)
            .GroupBy(x => new
            {
                x.AgentAggregate.ChiefAgentDividendPercentage,
                x.AgentAggregate.AgentDividendPercentageTreeData![0].AgentId
            }, (masterAgent, aggregates) => new
            {
                MasterAgentId = masterAgent.AgentId,
                Percentage = masterAgent.ChiefAgentDividendPercentage!.Value,
                DirectPlayerNetProfit =
                    aggregates.Sum(s => s.AgentAggregate.AgentId == masterAgent.AgentId
                        ? s.AgentAggregate.WinLossAmount - (s.AgentAggregate.RebateAmount +
                            s.AgentAggregate.PromotionAmount +
                            (s.BonusAggregate == null
                                ? 0M
                                : s.BonusAggregate.UpgradeAmount + s.BonusAggregate.SeasonAmount +
                                s.BonusAggregate.BirthdayAmount) +
                            s.AgentAggregate.BonusAdjustAmount - s.AgentAggregate.NoIncludeNetProfitBonusAdjustAmount)
                        : 0M),
                TeamNetProfit =
                    aggregates.Sum(s => s.AgentAggregate.AgentId != masterAgent.AgentId
                        ? s.AgentAggregate.WinLossAmount - (s.AgentAggregate.RebateAmount +
                            s.AgentAggregate.PromotionAmount +
                            (s.BonusAggregate == null
                                ? 0M
                                : s.BonusAggregate.UpgradeAmount + s.BonusAggregate.SeasonAmount +
                                s.BonusAggregate.BirthdayAmount) +
                            s.AgentAggregate.BonusAdjustAmount - s.AgentAggregate.NoIncludeNetProfitBonusAdjustAmount)
                        : 0M),
                CompanyAdjustment = aggregates.Sum(s => s.AgentAggregate.CompanyAdjustAmount),
                AgentProfit = aggregates.Sum(m
                    => m.AgentAggregate.AgentId == null
                        ? 0M
                        : (m.AgentAggregate.WinLossAmount - (m.AgentAggregate.RebateAmount +
                            m.AgentAggregate.PromotionAmount +
                            (m.BonusAggregate == null
                                ? 0M
                                : m.BonusAggregate.UpgradeAmount + m.BonusAggregate.SeasonAmount +
                                m.BonusAggregate.BirthdayAmount) +
                            m.AgentAggregate.BonusAdjustAmount -
                            m.AgentAggregate.NoIncludeNetProfitBonusAdjustAmount)) *
                        m.AgentAggregate.ChiefAgentDividendPercentage * 0.01M),
                CompanyProfit = aggregates.Sum(m
                    => m.AgentAggregate.AgentId == null
                        ? 0M
                        : (m.AgentAggregate.WinLossAmount - (m.AgentAggregate.RebateAmount +
                            m.AgentAggregate.PromotionAmount +
                            (m.BonusAggregate == null
                                ? 0M
                                : m.BonusAggregate.UpgradeAmount + m.BonusAggregate.SeasonAmount +
                                m.BonusAggregate.BirthdayAmount) +
                            m.AgentAggregate.BonusAdjustAmount -
                            m.AgentAggregate.NoIncludeNetProfitBonusAdjustAmount)) *
                        (100 - m.AgentAggregate.ChiefAgentDividendPercentage) * 0.01M)
            })
            .Join(_context.Agents,
                aggregate => aggregate.MasterAgentId,
                agent => agent.Id,
                (aggregate, agent) => new
                {
                    MasterAgentName = agent.Username,
                    aggregate.Percentage,
                    aggregate.DirectPlayerNetProfit,
                    aggregate.TeamNetProfit,
                    aggregate.CompanyAdjustment,
                    aggregate.AgentProfit,
                    aggregate.CompanyProfit
                })
            .CreatePagedResponseAsync(request)
            .ConfigureAwait(false);

        return new PagedResponse<MarketAgentProfitModel>
        {
            Valid = true,
            TotalCount = response.TotalCount,
            Items = response.Items?.Select(x => new MarketAgentProfitModel
            {
                MasterAgentName = x.MasterAgentName,
                Percentage = x.Percentage,
                DirectPlayerNetProfit = x.DirectPlayerNetProfit,
                TeamNetProfit = x.TeamNetProfit,
                CompanyAdjustment = x.CompanyAdjustment,
                AgentProfit = x.AgentProfit ?? 0M,
                CompanyProfit = x.CompanyProfit ?? 0M
            }).ToArray()
        };
    }
}