using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.PlayerDeposit;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core;

public class PlayerDepositFactory : IPlayerDepositFactory
{
    private readonly TgsContext _context;

    public PlayerDepositFactory(TgsContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<IEnumerable<PlayerFirstDepositDetailModel>> GetPlayerFirstDepositDetailsAsync(Guid brandId,
        Guid agentId, DateTimeOffset start, DateTimeOffset end)
    {
        const string spName = "select * from dbo.report_get_player_first_deposit_details(@_brand_id,@_agent_id,@_from,@_to)";
        var conn = _context.Database.GetDbConnection();
        var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
        return await conn.QueryAsync<PlayerFirstDepositDetailModel>(spName,
            new Dictionary<string, object>
            {
                { "_brand_id", brandId },
                { "_agent_id", agentId },
                { "_from", start },
                { "_to", end }
            }, transaction: transaction);
    }

    public async Task<IEnumerable<PlayerNoSecondDepositDetailModel>> GetPlayerNoSecondDepositDetailsAsync(Guid brandId,
        Guid agentId, DateTimeOffset start, DateTimeOffset end)
    {
        const string spName = "select * from dbo.report_get_player_no_second_deposit_details(@_brand_id,@_agent_id,@_from,@_to)";
        var conn = _context.Database.GetDbConnection();
        var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
        return await conn.QueryAsync<PlayerNoSecondDepositDetailModel>(spName,
            new Dictionary<string, object>
            {
                { "_brand_id", brandId },
                { "_agent_id", agentId },
                { "_from", start },
                { "_to", end }
            }, transaction: transaction);
    }

    public async Task<RoundAggregateModel[]> GenerateRoundAggregateAsync(DateTimeOffset start, DateTimeOffset end)
    {
        const string spName = "select * from dbo.report_generate_round_aggregates(@_from,@_to,@_report_date,@_created_on,@_hour,@_minute)";
        var conn = _context.Database.GetDbConnection();
        var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
        var result = await conn.QueryAsync<RoundAggregateModel>(spName,
            new Dictionary<string, object>
            {
                { "_from", start },
                { "_to", end },
                { "_report_date", new DateTimeOffset(start.Year, start.Month, start.Day, 0, 0, 0, start.Offset) },
                { "_created_on", DateTimeOffset.Now.ToOffset(start.Offset) },
                { "_hour", start.Hour },
                { "_minute", start.Minute }
            }, transaction: transaction);

        return result.ToArray();
    }

    public async Task<RoundAggregateModel[]> GetMinuteRoundAggregateAsync(DateTimeOffset start,
        DateTimeOffset end)
    {
        const string spName = "select * from dbo.report_get_round_and_set_game_setting_aggregates(@_from,@_to,@_report_date,@_hour,@_minute)";
        var conn = _context.Database.GetDbConnection();
        var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
        var result = await conn.QueryAsync<RoundAggregateModel>(spName,
            new Dictionary<string, object>
            {
                { "_from", start },
                { "_to", end },
                { "_report_date", new DateTimeOffset(start.Year, start.Month, start.Day, 0, 0, 0, start.Offset) },
                { "_hour", start.Hour },
                { "_minute", start.Minute }
            }, transaction: transaction);

        return result.ToArray();
    }
}