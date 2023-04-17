using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Market;

namespace Tgs.Report.Core
{
    internal class MarketFactory : IMarketFactory
    {
        private readonly TgsContext _context;

        public MarketFactory(TgsContext context) => _context = context;

        public async Task<IEnumerable<MarketAgentProfitDataModel>> GetAgentProfitsAsync(GetMarketAgentProfitsRequest request)
        {
            var spName = """
    WITH cte AS (
    SELECT 
        JSONB_PATH_QUERY(pa.AGENT_DIVIDEND_PERCENTAGE_TREE_DATA, '$[*] ? (@.ParentAgentId == null)') ->> 'AgentId' AS root_agent_id,
        pa.agent_id,
        pa.win_loss_amount,
        pa.rebate_amount,
        pa.promotion_amount,
        pa.bonus_adjust_amount,
        pa.no_include_net_profit_bonus_adjust_amount,
        pa.company_adjust_amount,
        pa.no_include_net_profit_company_adjust_amount,
        pa.chief_agent_dividend_percentage,
        COALESCE(pba.upgrade_amount,0.0) upgrade_amount,
        COALESCE(pba.season_amount,0.0) season_amount,
        COALESCE(pba.birthday_amount,0.0) birthday_amount
    FROM 
        report.player_aggregates pa
    LEFT JOIN 
        report.player_bonus_aggregates pba 
        ON pa.report_date = pba.report_date 
        AND pa."hour" = pba."hour" 
        AND pa.agent_id = pba.agent_id
        AND pa.player_code = pba.player_code
    WHERE 
        pa.brand_id = @brandId
        AND pa.report_date = @date
        AND pa.agent_id IS NOT NULL 
        AND JSONB_PATH_EXISTS(pa.AGENT_DIVIDEND_PERCENTAGE_TREE_DATA, '$[*] ? (@.ParentAgentId == null)'))
    SELECT * FROM
    (SELECT 
        a.username AS "MasterAgentName",
        chief_agent_dividend_percentage AS "Percentage",
        SUM(CASE WHEN root_agent_id = agent_id::text THEN
                win_loss_amount - rebate_amount - promotion_amount - upgrade_amount - season_amount - birthday_amount - (bonus_adjust_amount - no_include_net_profit_bonus_adjust_amount)
            ELSE 
                0.0 
            END
        ) AS "DirectPlayerNetProfit",
        SUM(CASE WHEN root_agent_id != agent_id::text THEN
                win_loss_amount - rebate_amount - promotion_amount - upgrade_amount - season_amount - birthday_amount - (bonus_adjust_amount - no_include_net_profit_bonus_adjust_amount)
            ELSE 
                0.0 
            END
        ) AS "TeamNetProfit",
        SUM(
            (win_loss_amount - rebate_amount - promotion_amount - upgrade_amount - season_amount - birthday_amount - (bonus_adjust_amount - no_include_net_profit_bonus_adjust_amount)) * chief_agent_dividend_percentage / 100.0
        ) AS "AgentProfit",
        SUM(company_adjust_amount) AS "CompanyAdjustment",
        SUM(
            ((win_loss_amount - rebate_amount - promotion_amount - upgrade_amount - season_amount - birthday_amount - (bonus_adjust_amount - no_include_net_profit_bonus_adjust_amount)) * (100.0 - chief_agent_dividend_percentage) / 100.0) - (company_adjust_amount - no_include_net_profit_company_adjust_amount)
        ) AS "CompanyProfit",
        count(*) over() AS "TotalCount"
    FROM 
        cte a1
    JOIN 
        dbo.agents a 
        ON a.id = a1.root_agent_id::uuid
    GROUP BY 
        1, 2) a1
    order by
      case when @isDesc = false AND lower(@sortBy) = 'masteragentname' then "MasterAgentName" end asc,
      case when @isDesc = true AND lower(@sortBy) = 'masteragentname' then "MasterAgentName" end desc,
      case when @isDesc = false AND lower(@sortBy) = 'percentage' then "Percentage" end asc,
      case when @isDesc = true AND lower(@sortBy) = 'percentage' then "Percentage" end desc,
      case when @isDesc = false AND lower(@sortBy) = 'directplayernetprofit' then "DirectPlayerNetProfit" end asc,
      case when @isDesc = true AND lower(@sortBy) = 'directplayernetprofit' then "DirectPlayerNetProfit" end desc,
      case when @isDesc = false AND lower(@sortBy) = 'teamnetprofit' then "TeamNetProfit" end asc,
      case when @isDesc = true AND lower(@sortBy) = 'teamnetprofit' then "TeamNetProfit" end desc,
      case when @isDesc = false AND lower(@sortBy) = 'agentprofit' then "AgentProfit" end asc,
      case when @isDesc = true AND lower(@sortBy) = 'agentprofit' then "AgentProfit" end desc,
      case when @isDesc = false AND lower(@sortBy) = 'companyadjustment' then "CompanyAdjustment" end asc,
      case when @isDesc = true AND lower(@sortBy) = 'companyadjustment' then "CompanyAdjustment" end desc,
      case when @isDesc = false AND lower(@sortBy) = 'companyprofit' then "CompanyProfit" end asc,
      case when @isDesc = true AND lower(@sortBy) = 'companyprofit' then "CompanyProfit" end desc
    offset (@pageNumber-1) * @pageSize 
    limit @pageSize;
    """;

            var conn = _context.Database.GetDbConnection();
            var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
            return await conn.QueryAsync<MarketAgentProfitDataModel>(spName,
                new Dictionary<string, object>
                {
                    { "brandId", request.BrandId },
                    { "date", request.Date.UtcDateTime },
                    { "sortBy", request.SortBy ?? "masteragentname" },
                    { "pageSize", request.ShowAll ? int.MaxValue : request.Rows },
                    { "pageNumber", request.ShowAll ? 1 : request.Page },
                    { "isDesc", request.IsDesc }
                }, transaction);

        }
    }
}
