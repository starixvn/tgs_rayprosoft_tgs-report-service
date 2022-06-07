using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel.Market;
using Tgs.Report.ViewModel.PlayerDeposit;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core
{
    public class PlayerDepositFactory : IPlayerDepositFactory
    {
        private readonly ITgsRepository _repository;

        public PlayerDepositFactory(ITgsRepository repository)
        {
            _repository = repository;
        }

        public async Task<RoundAggregateModel[]> GenerateRoundAggregateAsync(DateTimeOffset start, DateTimeOffset end)
        {
            const string spName = "dbo.report_generate_round_aggregates";
            var conn = _repository.GetDbConnection();
            var transaction = _repository.CurrentTransaction?.GetDbTransaction();
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
            var conn = _repository.GetDbConnection();
            var transaction = _repository.CurrentTransaction?.GetDbTransaction();
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

        public async Task<IEnumerable<PlayerFirstDepositDetailModel>> GetPlayerFirstDepositDetailsAsync(Guid brandId,Guid agentId, DateTimeOffset start, DateTimeOffset end)
        {
            const string spName = "dbo.report_get_player_first_deposit_details";
            var conn = _repository.GetDbConnection();
            var transaction = _repository.CurrentTransaction?.GetDbTransaction();
            return await conn.QueryAsync<PlayerFirstDepositDetailModel>(spName,
                new Dictionary<string, object>
                {
                    { "_brand_id",brandId },
                    { "_agent_id",agentId },
                    { "_from", start },
                    { "_to", end }
                }, commandType: CommandType.StoredProcedure, transaction: transaction);
        }

        public async Task<IEnumerable<PlayerNoSecondDepositDetailModel>> GetPlayerNoSecondDepositDetailsAsync(Guid brandId, Guid agentId, DateTimeOffset start, DateTimeOffset end)
        {
            const string spName = "dbo.report_get_player_no_second_deposit_details";
            var conn = _repository.GetDbConnection();
            var transaction = _repository.CurrentTransaction?.GetDbTransaction();
            return await conn.QueryAsync<PlayerNoSecondDepositDetailModel>(spName,
                new Dictionary<string, object>
                {
                    { "_brand_id",brandId },
                    { "_agent_id",agentId },
                    { "_from", start },
                    { "_to", end }
                }, commandType: CommandType.StoredProcedure, transaction: transaction);
        }
    }
}