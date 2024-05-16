using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core;

internal class AgentFactory(TgsContext context) : IAgentFactory
{
    public async Task<IEnumerable<AgentWinLossModel>> GetMasterAgentWinLossAsync(GetMasterAgentWinLossDataRequest request)
    {
        var spName = @"
WITH agent_data AS
(
         SELECT   agent_id,Jsonb_path_query(agent_dividend_percentage_tree_data, '$[*] ? (@.ParentAgentId == null)') ->> 'AgentId'                                                                                                   AS masterid ,
                  (Jsonb_path_query(agent_dividend_percentage_tree_data, '$[*] ? (@.ParentAgentId == null).DividendPercentage'))::NUMERIC                                                                                   AS agentdividendpercentage ,
                  SUM(turnover)                                                                                                                                                                                             AS turnover ,
                  SUM(win_loss_amount)                                                                                                                                                                                      AS win_loss_amount ,
                  SUM(rebate_amount + promotion_amount + upgrade_amount + season_amount + birthday_amount + (bonus_adjust_amount - no_include_net_profit_bonus_adjust_amount))                                              AS bonus_amount ,
	              SUM(win_loss_amount * platform_charge_percentage / 100)                                                                                                                                                   AS platform_charge_amount ,
                  SUM(round_count)                                                                                                                                                                                          AS round_count
         FROM     report.player_aggregates
         WHERE    brand_id = @brandid
         AND      report_date >= @from
         AND      report_date < @to
         AND      agent_id IS NOT NULL
         GROUP BY agent_id,masterid ,
                  agentdividendpercentage ) , 
		master_agent_data AS
(
         SELECT   masterid ,
                  agentdividendpercentage ,
                  SUM(turnover) turnover,
                  SUM(win_loss_amount) win_loss_amount,
                  SUM(bonus_amount) bonus_amount ,
	              SUM(platform_charge_amount) platform_charge_amount ,
                  SUM(round_count) round_count
         FROM     agent_data
         GROUP BY masterid ,
                  agentdividendpercentage ) ,
		game_type_fee as(
SELECT
	ad.masterid,
	pgra.agent_id,
	pgra.product_fee_percentage,
	pgra.game_type,
	sum(win_loss) win_loss
	FROM report.player_game_rebate_aggregates pgra
	join report.player_aggregates pa 
    on pa.brand_id = pgra.brand_id 
    and pa.agent_id = pgra.agent_id 
    and pa.player_code = pgra.player_code 
    and pa.report_date = pgra.report_date 
    and pa.""hour"" = pgra.""hour""
	join agent_data ad on ad.agent_id = pa.agent_id
  where pgra.brand_id = @brandid
  AND pgra.report_date >= @from
  AND pgra.report_date  < @to
	group by 1,2,3,4),
	fee as(
	select masterid,
	sum(cardGame + liveDealer + lottery + sports + slot + virtualSports) fee_amount
	from
	(select
	masterid,
	agent_id,
	case when coalesce(sum(win_loss) filter (where game_type =7),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type =7),0.0) end cardGame,
	case when coalesce(sum(win_loss) filter (where game_type =5),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type =5),0.0) end liveDealer,
	case when coalesce(sum(win_loss) filter (where game_type =13),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type =13),0.0) end lottery,
	case when coalesce(sum(win_loss) filter (where game_type = ANY(ARRAY[1,4])),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type = ANY(ARRAY[1,4])),0.0) end sports,
	case when coalesce(sum(win_loss) filter (where game_type =ANY(ARRAY[9,11,15,16])),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type =ANY(ARRAY[9,11,15,16])),0.0) end slot,
	case when coalesce(sum(win_loss) filter (where game_type =2),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type =2),0.0) end virtualSports
	from game_type_fee
	group by 1,2) a
	group by 1
	),
	agent_win_loss AS
(
         SELECT   ad.masterid AS id ,
                  a.username ,
                  'agent'                                                                AS membertype ,
                  TRUNC(SUM(ad.turnover), 6)                                                       AS turnover ,
                  TRUNC(-SUM(ad.win_loss_amount), 6)                                                AS winloss ,
                  TRUNC(SUM(ad.win_loss_amount * ad.agentdividendpercentage / 100.0), 6)           AS agentwinloss ,
                  TRUNC(SUM(ad.win_loss_amount * (100.0 - ad.agentdividendpercentage) / 100.0), 6) AS companywinloss ,
                  TRUNC(SUM(ad.bonus_amount), 6)                                                    AS special ,
                  TRUNC(-SUM(ad.bonus_amount * ad.agentdividendpercentage / 100.0), 6)              AS agentspecial ,
                  TRUNC(-SUM(ad.bonus_amount * (100.0 - ad.agentdividendpercentage) / 100.0), 6)    AS companyspecial ,
                  TRUNC(SUM(ad.platform_charge_amount), 6)                                          AS platformchargeamount ,
	              TRUNC(coalesce(SUM(f.fee_amount),0), 6)                                           AS feeamount ,
                  SUM(round_count)                                                           roundcount
         FROM     master_agent_data ad
         join     dbo.agents a
         ON       a.id = ad.masterid::uuid
	     left join     fee f on f.masterid::uuid = ad.masterid::uuid
         GROUP BY ad.masterid,
                  a.username )
SELECT *
FROM   agent_win_loss
WHERE turnover <> 0 OR winloss <> 0 OR special <> 0;";

        var conn = context.Database.GetDbConnection();
        var transaction = context.Database.CurrentTransaction?.GetDbTransaction();
        return await conn.QueryAsync<AgentWinLossModel>(spName,
            new Dictionary<string, object>
            {
                { "brandId", request.BrandId },
                { "from", request.From },
                { "to", request.To }
            }, transaction);
    }

    public async Task<IEnumerable<AgentWinLossModel>> GetAgentWinLossAsync(GetAgentWinLossDataRequest request)
    {
        var spName = $@"
WITH agent_data AS
(
         SELECT   agent_id,
	              Jsonb_path_query(agent_dividend_percentage_tree_data, '$[*] ? (@.AgentId == ""{request.AgentId}"")') ->> 'AgentId'                                                                                        AS masterid ,
                  (Jsonb_path_query(agent_dividend_percentage_tree_data, '$[*] ? (@.AgentId == ""{request.AgentId}"").DividendPercentage'))::NUMERIC                                                                        AS agentdividendpercentage ,
                  SUM(turnover)                                                                                                                                                                                             AS turnover ,
                  SUM(win_loss_amount)                                                                                                                                                                                      AS win_loss_amount ,
                  SUM(rebate_amount + promotion_amount + upgrade_amount + season_amount + birthday_amount + (bonus_adjust_amount - no_include_net_profit_bonus_adjust_amount))                                              AS bonus_amount ,
	              SUM(win_loss_amount * platform_charge_percentage / 100)                                                                                                                                                   AS platform_charge_amount ,
                  SUM(round_count)                                                                                                                                                                                          AS round_count
         FROM     report.player_aggregates
         WHERE    brand_id = @brandid
         AND report_date >= @from
         AND report_date  < @to
         AND      agent_id IS NOT NULL
         GROUP BY agent_id, masterid ,
                  agentdividendpercentage ) ,
	master_agent_data AS
(
         SELECT   masterid ,
                  agentdividendpercentage ,
                  SUM(turnover) turnover,
                  SUM(win_loss_amount) win_loss_amount,
                  SUM(bonus_amount) bonus_amount ,
	              SUM(platform_charge_amount) platform_charge_amount ,
                  SUM(round_count) round_count
         FROM     agent_data
         GROUP BY masterid ,
                  agentdividendpercentage ) ,
	game_type_fee as(
SELECT
	ad.masterid,
	pgra.agent_id,
	pgra.product_fee_percentage,
	pgra.game_type,
	sum(win_loss) win_loss
	FROM report.player_game_rebate_aggregates pgra
	join report.player_aggregates pa 
    on pa.brand_id = pgra.brand_id 
    and pa.agent_id = pgra.agent_id 
    and pa.player_code = pgra.player_code 
    and pa.report_date = pgra.report_date 
    and pa.""hour"" = pgra.""hour""
	join agent_data ad on ad.agent_id = pa.agent_id
  where pgra.brand_id = @brandid
  AND pgra.report_date >= @from
  AND pgra.report_date  < @to
	group by 1,2,3,4),
	fee as(
	select masterid,
	sum(cardGame + liveDealer + lottery + sports + slot + virtualSports) fee_amount
	from
	(select
	masterid,
	agent_id,
	case when coalesce(sum(win_loss) filter (where game_type =7),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type =7),0.0) end cardGame,
	case when coalesce(sum(win_loss) filter (where game_type =5),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type =5),0.0) end liveDealer,
	case when coalesce(sum(win_loss) filter (where game_type =13),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type =13),0.0) end lottery,
	case when coalesce(sum(win_loss) filter (where game_type = ANY(ARRAY[1,4])),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type = ANY(ARRAY[1,4])),0.0) end sports,
	case when coalesce(sum(win_loss) filter (where game_type =ANY(ARRAY[9,11,15,16])),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type =ANY(ARRAY[9,11,15,16])),0.0) end slot,
	case when coalesce(sum(win_loss) filter (where game_type =2),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type =2),0.0) end virtualSports
	from game_type_fee
	group by 1,2) a
	group by 1
	),
	agent_win_loss AS
(
         SELECT   ad.masterid AS id ,
                  a.username ,
                  'agent'                                                                AS membertype ,
                  TRUNC(SUM(ad.turnover), 6)                                                       AS turnover ,
                  TRUNC(-SUM(ad.win_loss_amount), 6)                                                AS winloss ,
                  TRUNC(SUM(ad.win_loss_amount * ad.agentdividendpercentage / 100.0), 6)           AS agentwinloss ,
                  TRUNC(SUM(ad.win_loss_amount * (100.0 - ad.agentdividendpercentage) / 100.0), 6) AS companywinloss ,
                  TRUNC(SUM(ad.bonus_amount), 6)                                                    AS special ,
                  TRUNC(-SUM(ad.bonus_amount * ad.agentdividendpercentage / 100.0), 6)              AS agentspecial ,
                  TRUNC(-SUM(ad.bonus_amount * (100.0 - ad.agentdividendpercentage) / 100.0), 6)    AS companyspecial ,
	              TRUNC(SUM(ad.platform_charge_amount), 6)                                          AS platformchargeamount ,
	              TRUNC(coalesce(SUM(f.fee_amount), 0), 6)                                          AS feeamount ,
                  SUM(round_count)                                                           roundcount
         FROM     master_agent_data ad
         join     dbo.agents a
         ON       a.id = ad.masterid::uuid
	     left join     fee f on f.masterid::uuid = ad.masterid::uuid
         GROUP BY ad.masterid,
                  a.username )
SELECT *,
       count(*) over() AS ""totalcount""
FROM   (
              SELECT *
              FROM   agent_win_loss
              WHERE turnover <> 0 OR winloss <> 0 OR special <> 0) a1;";

        var conn = context.Database.GetDbConnection();
        var transaction = context.Database.CurrentTransaction?.GetDbTransaction();
        return await conn.QueryAsync<AgentWinLossModel>(spName,
            new Dictionary<string, object>
            {
                { "brandId", request.BrandId },
                { "from", request.From },
                { "to", request.To }
            }, transaction);
    }

    public async Task<IEnumerable<AgentWinLossModel>> GetDownLineAgentWinLossAsync(GetDownLineAgentWinLossDataRequest request)
    {
        var spName = $@"
WITH agent_data AS
(
         SELECT   agent_id,Jsonb_path_query(agent_dividend_percentage_tree_data, '$[*] ? (@.ParentAgentId == ""{request.AgentId}"")') ->> 'AgentId'                                                                                  AS childagentid,
                  (Jsonb_path_query(agent_dividend_percentage_tree_data, '$[*] ? (@.ParentAgentId == ""{request.AgentId}"").DividendPercentage'))::NUMERIC                                                                  AS agentdividendpercentage,
                  SUM(turnover)                                                                                                                                                                                             AS turnover,
                  SUM(win_loss_amount)                                                                                                                                                                                      AS win_loss_amount,
                  SUM(rebate_amount + promotion_amount + upgrade_amount + season_amount + birthday_amount + (bonus_adjust_amount - no_include_net_profit_bonus_adjust_amount))                                              AS bonus_amount,
	              SUM(win_loss_amount * platform_charge_percentage / 100)                                                                                                                                                   AS platform_charge_amount ,
                  SUM(round_count)                                                                                                                                                                                          AS round_count
         FROM     report.player_aggregates
         WHERE    report_date >= @FROM
         AND      report_date < @TO
         AND      brand_id = @brandid
         AND      agent_id IS NOT NULL
         GROUP BY agent_id,childagentid,
                  agentdividendpercentage ), 
child_agent_data AS
(
         SELECT   childagentid ,
                  agentdividendpercentage ,
                  SUM(turnover) turnover,
                  SUM(win_loss_amount) win_loss_amount,
                  SUM(bonus_amount) bonus_amount ,
	              SUM(platform_charge_amount) platform_charge_amount ,
                  SUM(round_count) round_count
         FROM     agent_data
         GROUP BY childagentid ,
                  agentdividendpercentage ) ,
	game_type_fee as(
SELECT
	ad.childagentid,
	pgra.agent_id,
	pgra.product_fee_percentage,
	pgra.game_type,
	sum(win_loss) win_loss
	FROM report.player_game_rebate_aggregates pgra
	join report.player_aggregates pa 
    on pa.brand_id = pgra.brand_id 
    and pa.agent_id = pgra.agent_id 
    and pa.player_code = pgra.player_code 
    and pa.report_date = pgra.report_date 
    and pa.""hour"" = pgra.""hour""
	join agent_data ad on ad.agent_id = pa.agent_id
  where pgra.brand_id = @brandid
  AND pgra.report_date >= @from
  AND pgra.report_date  < @to
	group by 1,2,3,4),
	fee as(
	select childagentid,
	sum(cardGame + liveDealer + lottery + sports + slot + virtualSports) fee_amount
	from
	(select
	childagentid,
	agent_id,
	case when coalesce(sum(win_loss) filter (where game_type =7),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type =7),0.0) end cardGame,
	case when coalesce(sum(win_loss) filter (where game_type =5),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type =5),0.0) end liveDealer,
	case when coalesce(sum(win_loss) filter (where game_type =13),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type =13),0.0) end lottery,
	case when coalesce(sum(win_loss) filter (where game_type = ANY(ARRAY[1,4])),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type = ANY(ARRAY[1,4])),0.0) end sports,
	case when coalesce(sum(win_loss) filter (where game_type =ANY(ARRAY[9,11,15,16])),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type =ANY(ARRAY[9,11,15,16])),0.0) end slot,
	case when coalesce(sum(win_loss) filter (where game_type =2),0.0) < 0 then 0.0 else coalesce(sum(win_loss * product_fee_percentage / 100) filter (where game_type =2),0.0) end virtualSports
	from game_type_fee
	group by 1,2) a
	group by 1
	),
agent_win_loss AS
(
         SELECT   ad.childagentid AS id,
                  a.username,
                  'agent'                                                                AS membertype,
                  SUM(ad.turnover)                                                       AS turnover,
                  -SUM(ad.win_loss_amount)                                                AS winloss,
                  TRUNC(SUM(ad.win_loss_amount * ad.agentdividendpercentage / 100.0), 6)           AS agentwinloss,
                  TRUNC(SUM(ad.win_loss_amount * (100.0 - ad.agentdividendpercentage) / 100.0), 6) AS companywinloss,
                  TRUNC(SUM(ad.bonus_amount), 6)                                          AS special ,
                  TRUNC(-SUM(ad.bonus_amount * ad.agentdividendpercentage / 100.0), 6)              AS agentspecial ,
                  TRUNC(-SUM(ad.bonus_amount * (100.0 - ad.agentdividendpercentage) / 100.0), 6)    AS companyspecial ,
                  TRUNC(SUM(ad.platform_charge_amount), 6)                                AS platformchargeamount ,
	              TRUNC(coalesce(SUM(f.fee_amount), 0), 6)                                AS feeamount ,
                  SUM(round_count)                                                       AS roundcount
         FROM     child_agent_data ad
         join     dbo.agents a
         ON       a.id = ad.childagentid::uuid
         left join     fee f on f.childagentid::uuid = ad.childagentid::uuid
         GROUP BY ad.childagentid,
                  a.username ), player_win_loss AS
(
         SELECT   pa.player_code                                                                                                                                                                                                        AS id,
                  p.username                                                                                                                                                                                                        AS username,
                  'player'                                                                                                                                                                                                        AS membertype,
                  TRUNC(SUM(pa.turnover), 6)                                                                                                                                                                                                        AS turnover,
                  TRUNC(-SUM(pa.win_loss_amount), 6)                                                                                                                                                                                                        AS winloss,
                  TRUNC(SUM(pa.win_loss_amount * pa.agent_dividend_percentage / 100.0), 6)                                                                                                                                                                                               AS agentwinloss,
                  TRUNC(SUM(pa.win_loss_amount * (100.0 - pa.agent_dividend_percentage) / 100.0), 6)                                                                                                                                                                                     AS companywinloss,
                  TRUNC(SUM(rebate_amount      + promotion_amount + upgrade_amount + season_amount + birthday_amount + (bonus_adjust_amount - no_include_net_profit_bonus_adjust_amount)), 6)                                               AS special ,
                  TRUNC(-SUM((rebate_amount + promotion_amount + upgrade_amount + season_amount + birthday_amount + (bonus_adjust_amount - no_include_net_profit_bonus_adjust_amount)) * pa.agent_dividend_percentage / 100.0), 6)           AS agentspecial ,
                  TRUNC(-SUM((rebate_amount + promotion_amount + upgrade_amount + season_amount + birthday_amount + (bonus_adjust_amount - no_include_net_profit_bonus_adjust_amount)) * (100.0 - pa.agent_dividend_percentage) / 100.0), 6) AS companyspecial ,
                  0                                AS platformchargeamount ,
	              0                                AS feeamount ,
                  SUM(round_count)                                                                                                                                                                                                        AS roundcount
         FROM     report.player_aggregates pa
         join     dbo.players p
         ON       p.player_code = pa.player_code
         WHERE    report_date >= @FROM
         AND      report_date < @TO
         AND      pa.brand_id = @brandid
         AND      pa.agent_id = @agentid
         GROUP BY pa.player_code,
                  p.username )
SELECT *
FROM   agent_win_loss
UNION
SELECT *
FROM   player_win_loss
WHERE turnover <> 0 OR winloss <> 0 OR special <> 0;";

        var conn = context.Database.GetDbConnection();
        var transaction = context.Database.CurrentTransaction?.GetDbTransaction();
        return await conn.QueryAsync<AgentWinLossModel>(spName,
            new Dictionary<string, object>
            {
                { "brandId", request.BrandId },
                { "agentId", request.AgentId },
                { "from", request.From },
                { "to", request.To }
            }, transaction);
    }

    public async Task<AgentPlayerSpecialFeesModel> GetAgentPlayerSpecialFeesAsync(
        GetAgentPlayerSpecialFeesDataRequest request)
    {
        var spName = $@"
SELECT
  TRUNC(Sum(rebate_amount       + promotion_amount + upgrade_amount + season_amount + birthday_amount), 6) as bonus
, TRUNC(Sum(bonus_adjust_amount - no_include_net_profit_bonus_adjust_amount), 6)                           as bonusAdjust
FROM
  report.player_aggregates
WHERE
  brand_id         = @brandid
  AND report_date >= @from
  AND report_date  < @to
  AND Jsonb_path_exists(agent_dividend_percentage_tree_data, '$[*] ? (@.AgentId == ""{request.AgentId}"")')
  AND agent_id IS NOT NULL;";

        var conn = context.Database.GetDbConnection();
        var transaction = context.Database.CurrentTransaction?.GetDbTransaction();
        var response = (await conn.QueryAsync<AgentPlayerSpecialFeesModel>(spName,
            new Dictionary<string, object>
            {
                { "brandId", request.BrandId },
                { "from", request.From },
                { "to", request.To }
            }, transaction)).FirstOrDefault();

        return new AgentPlayerSpecialFeesModel
        {
            Bonus = response?.Bonus ?? 0M,
            BonusAdjust = response?.BonusAdjust ?? 0M,
        };
    }

    public Task<IEnumerable<SpecialDetailDataModel>> GetSpecialDetailAsync(GetSpecialDetailDataRequest request)
    {
        var spName = $@"
SELECT
  report_date                                                                                                                                                                                     AS ReportDate
, Jsonb_path_query(agent_dividend_percentage_tree_data, '$[*] ? (@.AgentId == ""{request.AgentId}"")') ->> 'AgentId'                                                                              AS AgentId
, ( Jsonb_path_query(agent_dividend_percentage_tree_data, '$[*] ? (@.AgentId == ""{request.AgentId}"").DividendPercentage') ) :: NUMERIC                                                          AS Percentage
, TRUNC(Sum(win_loss_amount), 6)                                                                                                                                                                            AS WinLoss
, TRUNC(Sum(rebate_amount + promotion_amount + upgrade_amount + season_amount + birthday_amount + ( bonus_adjust_amount - no_include_net_profit_bonus_adjust_amount )), 6) AS BonusAmount
FROM
  report.player_aggregates
WHERE
  report_date           >= @from
  AND report_date        < @to
  AND brand_id           = @brandid
  AND agent_id IS NOT NULL
GROUP BY
  1
, 2
, 3;";

        var conn = context.Database.GetDbConnection();
        var transaction = context.Database.CurrentTransaction?.GetDbTransaction();
        return conn.QueryAsync<SpecialDetailDataModel>(spName,
            new Dictionary<string, object>
            {
                { "brandId", request.BrandId },
                { "from", request.From },
                { "to", request.To }
            }, transaction);
    }

    public async IAsyncEnumerable<ProductFeeDataModel> GetAgentPlayerSpecialProductFeeAsync(
        GetAgentPlayerSpecialProductFeeDataRequest request)
    {
        var spName = $@"
SELECT pgra.agent_id AgentId,
	pgra.product_fee_percentage ProductFeePercentage,
	pgra.game_type GameType,
	sum(win_loss) WinLoss
	FROM report.player_game_rebate_aggregates pgra
	join report.player_aggregates pa 
    on pa.brand_id = pgra.brand_id 
    and pa.agent_id = pgra.agent_id 
    and pa.player_code = pgra.player_code 
    and pa.report_date = pgra.report_date 
    and pa.""hour"" = pgra.""hour""
  where pgra.brand_id = @brandid
  AND pgra.report_date >= @from
  AND pgra.report_date  < @to
  AND Jsonb_path_exists(pa.agent_dividend_percentage_tree_data, '$[*] ? (@.AgentId == ""{request.AgentId}"")')
  AND pgra.agent_id IS NOT NULL
  group by 1,2,3;";

        var conn = context.Database.GetDbConnection();
        var transaction = context.Database.CurrentTransaction?.GetDbTransaction();
        var response = await conn.QueryAsync<ProductFeeDataModel>(spName,
            new Dictionary<string, object>
            {
                { "brandId", request.BrandId },
                { "from", request.From },
                { "to", request.To }
            }, transaction);

        foreach (var item in response)
        {
            yield return item;
        }
    }
}