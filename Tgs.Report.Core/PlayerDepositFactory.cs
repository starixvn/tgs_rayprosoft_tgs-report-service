using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.PlayerDeposit;

namespace Tgs.Report.Core;

public class PlayerDepositFactory(TgsContext context) : IPlayerDepositFactory
{
    private readonly TgsContext _context = context ?? throw new ArgumentNullException(nameof(context));

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
}