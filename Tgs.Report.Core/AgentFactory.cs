using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Core.Queries.Agent;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core;

internal class AgentFactory : IAgentFactory
{
    private readonly TgsContext _context;

    public AgentFactory(TgsContext context) => _context = context;

    public async Task<IEnumerable<AgentWinLossDataModel>> GetMasterAgentWinLossAsync(GetMasterAgentWinLossDataRequest request)
    {
        var spName = @"WITH AGENT_DATA AS
  (
    SELECT
    JSONB_PATH_QUERY(AGENT_DIVIDEND_PERCENTAGE_TREE_DATA, '$[*] ? (@.ParentAgentId == null)') ->> 'AgentId'    AS masterId
    , (JSONB_PATH_QUERY(AGENT_DIVIDEND_PERCENTAGE_TREE_DATA, '$[*] ? (@.ParentAgentId == null).DividendPercentage'))::numeric                                                                            AS agentDividendPercentage
    , SUM(TURNOVER)                                                                                              AS turnover
    , SUM(WIN_LOSS_AMOUNT)                                                                                       AS win_loss_amount
    , SUM(rebate_amount + promotion_amount + upgrade_amount + season_amount + birthday_amount) AS bonus_amount
    , SUM(round_count)                                                                                           AS RoundCount
    FROM
      REPORT.PLAYER_AGGREGATES
    WHERE
      brand_id             = @brandId
      AND REPORT_DATE     >= @from
      AND REPORT_DATE      < @to
      AND agent_id is not null
    GROUP BY
      masterId
    , agentDividendPercentage
  )
, AGENT_WIN_LOSS AS
  (
    SELECT
      AD.masterId AS Id
    ,  A.Username
    , 'agent'                                                                AS MemberType
    , SUM(AD.TURNOVER)                                                       AS Turnover
    , SUM(AD.WIN_LOSS_AMOUNT)                                                AS WinLoss
    , SUM(AD.WIN_LOSS_AMOUNT * AD.agentDividendPercentage / 100.0)           AS AgentWinLoss
    , SUM(AD.WIN_LOSS_AMOUNT * (100.0 - AD.agentDividendPercentage) / 100.0) AS CompanyWinLoss
    , SUM(AD.bonus_amount)                                                   AS Special
    , SUM(AD.bonus_amount    * AD.agentDividendPercentage / 100.0)           AS AgentSpecial
    , SUM(AD.bonus_amount    * (100.0 - AD.agentDividendPercentage) / 100.0) AS CompanySpecial
    , SUM(RoundCount)                                                           RoundCount
    FROM
      AGENT_DATA AD
      JOIN
        DBO.AGENTS A
        ON
          A.ID = AD.masterId::UUID
    GROUP BY
      AD.masterId,A.Username
  )
SELECT *,count(*) over() as ""TotalCount"" from (
SELECT *
FROM
  AGENT_WIN_LOSS ) a1
ORDER BY
  CASE
    WHEN @isDesc         = false
      AND lower(@sortBy) = 'username'
      THEN Username
  END ASC
, CASE
    WHEN @isDesc         = false
      AND lower(@sortBy) = 'memberType'
      THEN MemberType
  END ASC
, CASE
    WHEN @isDesc         = true
      AND lower(@sortBy) = 'username'
      THEN Username
  END DESC
, CASE
    WHEN @isDesc         = true
      AND lower(@sortBy) = 'memberType'
      THEN MemberType
  END DESC
OFFSET (@pageNumber - 1) * @pageSize
LIMIT @pageSize;";

        var conn = _context.Database.GetDbConnection();
        var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
        return await conn.QueryAsync<AgentWinLossDataModel>(spName,
            new Dictionary<string, object>
            {
                { "brandId", request.BrandId },
                { "from", request.From },
                { "to", request.To },
                { "sortBy", request.SortBy ?? "username" },
                { "pageSize", request.Rows },
                { "pageNumber", request.Page },
                { "isDesc", request.IsDesc }
            }, transaction);
    }

    public async Task<IEnumerable<AgentWinLossDataModel>> GetAgentWinLossAsync(GetAgentWinLossDataRequest request)
    {
        var spName = $@"WITH AGENT_DATA AS
  (
    SELECT
    JSONB_PATH_QUERY(AGENT_DIVIDEND_PERCENTAGE_TREE_DATA, '$[*] ? (@.AgentId == ""{request.AgentId}"")') ->> 'AgentId'    AS masterId
    , (JSONB_PATH_QUERY(AGENT_DIVIDEND_PERCENTAGE_TREE_DATA, '$[*] ? (@.AgentId == ""{request.AgentId}"").DividendPercentage'))::numeric                                                                            AS agentDividendPercentage
    , SUM(TURNOVER)                                                                                              AS turnover
    , SUM(WIN_LOSS_AMOUNT)                                                                                       AS win_loss_amount
    , SUM(rebate_amount + promotion_amount + upgrade_amount + season_amount + birthday_amount) AS bonus_amount
    , SUM(round_count)                                                                                           AS RoundCount
    FROM
      REPORT.PLAYER_AGGREGATES
    WHERE
      brand_id             = @brandId
      AND REPORT_DATE     >= @from
      AND REPORT_DATE      < @to
      AND agent_id is not null
    GROUP BY
      masterId
    , agentDividendPercentage
  )
, AGENT_WIN_LOSS AS
  (
    SELECT
      AD.masterId AS Id
    ,  A.Username
    , 'agent'                                                                AS MemberType
    , SUM(AD.TURNOVER)                                                       AS Turnover
    , SUM(AD.WIN_LOSS_AMOUNT)                                                AS WinLoss
    , SUM(AD.WIN_LOSS_AMOUNT * AD.agentDividendPercentage / 100.0)           AS AgentWinLoss
    , SUM(AD.WIN_LOSS_AMOUNT * (100.0 - AD.agentDividendPercentage) / 100.0) AS CompanyWinLoss
    , SUM(AD.bonus_amount)                                                   AS Special
    , SUM(AD.bonus_amount    * AD.agentDividendPercentage / 100.0)           AS AgentSpecial
    , SUM(AD.bonus_amount    * (100.0 - AD.agentDividendPercentage) / 100.0) AS CompanySpecial
    , SUM(RoundCount)                                                           RoundCount
    FROM
      AGENT_DATA AD
      JOIN
        DBO.AGENTS A
        ON
          A.ID = AD.masterId::UUID
    GROUP BY
      AD.masterId,A.Username
  )
SELECT *,count(*) over() as ""TotalCount"" from (
SELECT *
FROM
  AGENT_WIN_LOSS ) a1
ORDER BY
  CASE
    WHEN @isDesc         = false
      AND lower(@sortBy) = 'username'
      THEN Username
  END ASC
, CASE
    WHEN @isDesc         = false
      AND lower(@sortBy) = 'memberType'
      THEN MemberType
  END ASC
, CASE
    WHEN @isDesc         = true
      AND lower(@sortBy) = 'username'
      THEN Username
  END DESC
, CASE
    WHEN @isDesc         = true
      AND lower(@sortBy) = 'memberType'
      THEN MemberType
  END DESC
OFFSET (@pageNumber - 1) * @pageSize
LIMIT @pageSize;";

        var conn = _context.Database.GetDbConnection();
        var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
        return await conn.QueryAsync<AgentWinLossDataModel>(spName,
            new Dictionary<string, object>
            {
                { "brandId", request.BrandId },
                { "from", request.From },
                { "to", request.To },
                { "sortBy", request.SortBy ?? "username" },
                { "pageSize", request.Rows },
                { "pageNumber", request.Page },
                { "isDesc", request.IsDesc }
            }, transaction);
    }

    public async Task<IEnumerable<AgentWinLossDataModel>> GetDownLineAgentWinLossAsync(GetDownLineAgentWinLossDataRequest request)
    {
        var spName = $@"WITH AGENT_DATA AS (
  SELECT
    JSONB_PATH_QUERY(AGENT_DIVIDEND_PERCENTAGE_TREE_DATA, '$[*] ? (@.ParentAgentId == ""{request.AgentId}"")') ->> 'AgentId' AS CHILDAGENTID,
    (JSONB_PATH_QUERY(AGENT_DIVIDEND_PERCENTAGE_TREE_DATA, '$[*] ? (@.ParentAgentId == ""{request.AgentId}"").DividendPercentage'))::numeric AS AGENTDIVIDENDPERCENTAGE,
    SUM(TURNOVER) AS TURNOVER,
    SUM(WIN_LOSS_AMOUNT) AS WIN_LOSS_AMOUNT,
    SUM(rebate_amount + promotion_amount + upgrade_amount + season_amount + birthday_amount) AS bonus_amount,
    SUM(round_count)                                                                                           AS round_count
  FROM
    REPORT.PLAYER_AGGREGATES
  WHERE
    REPORT_DATE >= @from AND REPORT_DATE < @to
    AND brand_id = @brandId
	AND agent_id is not null
  GROUP BY
    CHILDAGENTID, AGENTDIVIDENDPERCENTAGE
), AGENT_WIN_LOSS AS (
  SELECT
    AD.CHILDAGENTID AS Id,
    A.Username,
    'agent' AS MemberType,
    SUM(AD.TURNOVER) AS Turnover,
    SUM(AD.WIN_LOSS_AMOUNT) AS WinLoss,
    SUM(AD.WIN_LOSS_AMOUNT * AD.AGENTDIVIDENDPERCENTAGE / 100.0) AS AgentWinLoss,
    SUM(AD.WIN_LOSS_AMOUNT * (100.0 - AD.AGENTDIVIDENDPERCENTAGE) / 100.0) AS CompanyWinLoss,
    SUM(AD.bonus_amount) AS Special
, SUM(AD.bonus_amount    * AD.agentDividendPercentage / 100.0)           AS AgentSpecial
    , SUM(AD.bonus_amount    * (100.0 - AD.agentDividendPercentage) / 100.0) AS CompanySpecial
,SUM(round_count)                                                                                           AS RoundCount
  FROM
    AGENT_DATA AD
    JOIN DBO.AGENTS A ON A.ID = AD.CHILDAGENTID::UUID
  GROUP BY
    AD.CHILDAGENTID,A.Username
), PLAYER_WIN_LOSS AS (
  SELECT
    PA.player_code AS Id,
    P.USERNAME AS Username,
    'player' AS MemberType,
    SUM(PA.TURNOVER) AS Turnover,
    SUM(PA.WIN_LOSS_AMOUNT) AS WinLoss,
    SUM(PA.WIN_LOSS_AMOUNT * PA.agent_dividend_percentage / 100.0) AS AgentWinLoss,
    SUM(PA.WIN_LOSS_AMOUNT * (100.0 - PA.agent_dividend_percentage) / 100.0) AS CompanyWinLoss,
    SUM(rebate_amount + promotion_amount + upgrade_amount + season_amount + birthday_amount) AS Special
, SUM((rebate_amount + promotion_amount + upgrade_amount + season_amount + birthday_amount)    * PA.agent_dividend_percentage / 100.0)           AS AgentSpecial
    , SUM((rebate_amount + promotion_amount + upgrade_amount + season_amount + birthday_amount)    * (100.0 - PA.agent_dividend_percentage) / 100.0) AS CompanySpecial
,SUM(round_count)                                                                                           AS RoundCount
  FROM
    REPORT.PLAYER_AGGREGATES PA
    JOIN DBO.PLAYERS P ON P.PLAYER_CODE = PA.PLAYER_CODE
  WHERE
    REPORT_DATE >= @from AND REPORT_DATE < @to
    AND pa.brand_id = @brandId
    AND pa.AGENT_ID = @agentId
  GROUP BY
    PA.player_code,P.USERNAME
)
SELECT *,count(*) over() as ""TotalCount"" from (
SELECT *
FROM AGENT_WIN_LOSS
UNION
SELECT *
FROM PLAYER_WIN_LOSS) a1
order by
	  case when @isDesc = false AND lower(@sortBy) = 'username' then Username end asc,
	  case when @isDesc = false AND lower(@sortBy) = 'memberType'	then MemberType	end asc,
	  case when @isDesc = true AND lower(@sortBy) = 'username' then Username end desc,
	  case when @isDesc = true AND lower(@sortBy) = 'memberType' then MemberType end desc
    offset (@pageNumber-1) * @pageSize 
	limit @pageSize;";

        var conn = _context.Database.GetDbConnection();
        var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
        return await conn.QueryAsync<AgentWinLossDataModel>(spName,
            new Dictionary<string, object>
            {
                { "brandId", request.BrandId },
                { "agentId", request.AgentId },
                { "from", request.From },
                { "to", request.To },
                { "sortBy", request.SortBy ?? "username" },
                { "pageSize", request.Rows },
                { "pageNumber", request.Page },
                { "isDesc", request.IsDesc }
            }, transaction);
    }
}