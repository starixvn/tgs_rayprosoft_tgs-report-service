using Dapper;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core;

public class AgentFactory : IAgentFactory
{
    private readonly TgsContext _context;

    public AgentFactory(TgsContext context) => _context = context;

    public async Task<IEnumerable<AgentContributionDataModel>> GetAgentContributionAsync(GetAgentContributionsDataRequest request)
    {
        var spName = $@"
WITH agent_data AS
  (
    SELECT
      (Jsonb_path_query(agent_dividend_percentage_tree_data, '$[*] ? (@.ParentAgentId == ""{request.AgentId}"")') ->> 'AgentId')::uuid                                              AS masterId
    , agent_id AS agentId
    , SUM(turnover)                                                                                                                                                                          AS turnover
    , SUM(original_turnover)                                                                                                                                                                 AS originalTurnover
    , SUM(win_loss_amount)                                                                                                                                                                   AS winLossAmount
    , SUM(bet_amount)                                                                                                                                                                        AS betAmount
    , SUM(company_adjust_amount - no_include_net_profit_company_adjust_amount)                                                                                                               AS companyAdjustAmount
    , SUM(agent_adjust_amount   - no_include_net_profit_agent_adjust_amount)                                                                                                                 AS agentAdjustAmount
    , SUM(bonus_adjust_amount   - no_include_net_profit_bonus_adjust_amount)                                                                                                                 AS bonusAdjustAmount
    , SUM(rebate_amount         + promotion_amount + upgrade_amount + season_amount + birthday_amount)                                                                                       AS bonusAmount
    , SUM(win_loss_amount       - (rebate_amount + promotion_amount + upgrade_amount + season_amount + birthday_amount + (bonus_adjust_amount - no_include_net_profit_bonus_adjust_amount))) AS netProfit
    FROM
      report.player_aggregates
    WHERE
      brand_id               = @brandId
      AND report_date       >= @from
      AND report_date        < @to
      AND agent_id IS NOT NULL
      AND Jsonb_path_exists(agent_dividend_percentage_tree_data, '$[*] ? (@.ParentAgentId == ""{request.AgentId}"")')
    GROUP BY
      masterId, agent_id
  )
SELECT
  a.username
, ad.*
FROM
  agent_data ad
  join
    dbo.agents a
    ON
      a.id = ad.masterId::uuid;";

        var conn = _context.Database.GetDbConnection();
        var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

        return await conn.QueryAsync<AgentContributionDataModel>(spName,
            new Dictionary<string, object>
            {
                { "brandId", request.BrandId },
                { "agentId", request.AgentId },
                { "from", request.From },
                { "to", request.To }
            }, transaction);
    }
}
