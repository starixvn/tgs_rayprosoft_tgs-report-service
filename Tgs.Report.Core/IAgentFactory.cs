using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Core.Queries.Agent;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core;

public interface IAgentFactory
{
    Task<IEnumerable<AgentWinLossDataModel>> GetAgentWinLossAsync(GetAgentWinLossRequest request);
}

internal class AgentFactory : IAgentFactory
{
    private readonly TgsContext _context;

    public AgentFactory(TgsContext context) => _context = context;

    public async Task<IEnumerable<AgentWinLossDataModel>> GetAgentWinLossAsync(GetAgentWinLossRequest request)
    {
        var spName = $@"WITH AGENT_DATA AS (
  SELECT
    JSONB_PATH_QUERY(AGENT_DIVIDEND_PERCENTAGE_TREE_DATA, '$[*] ? (@.ParentAgentId == ""{request.AgentId}"")') ->> 'AgentId' AS CHILDAGENTID,
    (JSONB_PATH_QUERY(AGENT_DIVIDEND_PERCENTAGE_TREE_DATA, '$[*] ? (@.AgentId == ""{request.AgentId}"").DividendPercentage'))::numeric AS AGENTDIVIDENDPERCENTAGE,
    SUM(TURNOVER) AS TURNOVER,
    SUM(WIN_LOSS_AMOUNT) AS WIN_LOSS_AMOUNT
  FROM
    REPORT.PLAYER_AGGREGATES
  WHERE
    REPORT_DATE >= @from AND REPORT_DATE < @to
	and win_loss_amount <>0
  GROUP BY
    CHILDAGENTID, AGENTDIVIDENDPERCENTAGE
), AGENT_WIN_LOSS AS (
  SELECT
    A.Username,
    'agent' AS MemberType,
    SUM(AD.TURNOVER) AS Turnover,
    SUM(AD.WIN_LOSS_AMOUNT) AS WinLoss,
    SUM(AD.WIN_LOSS_AMOUNT * AD.AGENTDIVIDENDPERCENTAGE / 100.0) AS AgentWinLoss,
    SUM(AD.WIN_LOSS_AMOUNT * (100.0 - AD.AGENTDIVIDENDPERCENTAGE) / 100.0) AS CompanyWinLoss
  FROM
    AGENT_DATA AD
    JOIN DBO.AGENTS A ON A.ID = AD.CHILDAGENTID::UUID
  GROUP BY
    A.Username
), PLAYER_WIN_LOSS AS (
  SELECT
    P.USERNAME AS Username,
    'player' AS MemberType,
    SUM(PA.TURNOVER) AS Turnover,
    SUM(PA.WIN_LOSS_AMOUNT) AS WinLoss,
    SUM(PA.WIN_LOSS_AMOUNT * PA.agent_dividend_percentage / 100.0) AS AgentWinLoss,
    SUM(PA.WIN_LOSS_AMOUNT * (100.0 - PA.agent_dividend_percentage) / 100.0) AS CompanyWinLoss
  FROM
    REPORT.PLAYER_AGGREGATES PA
    JOIN DBO.PLAYERS P ON P.PLAYER_CODE = PA.PLAYER_CODE
  WHERE
    REPORT_DATE >= @from AND REPORT_DATE < @to
    AND pa.AGENT_ID = @agentId
	and win_loss_amount <>0
  GROUP BY
    P.USERNAME
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