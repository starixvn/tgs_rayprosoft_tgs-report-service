using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core
{
    public class RoundFactory : IRoundFactory
    {
        private readonly TgsContext _context;

        public RoundFactory(TgsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<RoundAggregateModel[]> GenerateRoundAggregateAsync(DateTimeOffset start, DateTimeOffset end)
        {
            const string spName = "dbo.report_generate_round_aggregates";
            var conn = _context.Database.GetDbConnection();
            var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
            var result = await conn.QueryAsync<RoundAggregateModel>(spName,
                new Dictionary<string, object>
                {
                    { "_from", start },
                    { "_to", end },
                    {
                        "_report_date",
                        new DateTimeOffset(start.Year, start.Month, start.Day, 0, 0, 0, start.Offset)
                    },
                    { "_created_on", DateTimeOffset.Now.ToOffset(start.Offset) },
                    { "_hour", start.Hour },
                    { "_minute", start.Minute }
                }, commandType: CommandType.StoredProcedure, transaction: transaction);

            return result.ToArray();
        }

        public async Task<RoundAggregateModel[]> GetMinuteRoundAggregateAsync(DateTimeOffset start,
            DateTimeOffset end)
        {
            const string spName = "dbo.report_get_round_and_set_game_setting_aggregates";
            var conn = _context.Database.GetDbConnection();
            var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
            var result = await conn.QueryAsync<RoundAggregateModel>(spName,
                new Dictionary<string, object>
                {
                    { "_from", start },
                    { "_to", end },
                    {
                        "_report_date",
                        new DateTimeOffset(start.Year, start.Month, start.Day, 0, 0, 0, start.Offset)
                    },
                    { "_hour", start.Hour },
                    { "_minute", start.Minute }
                }, commandType: CommandType.StoredProcedure, transaction: transaction);

            return result.ToArray();
        }
    }
}