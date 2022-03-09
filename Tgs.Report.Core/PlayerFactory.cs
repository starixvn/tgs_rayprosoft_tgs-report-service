using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core
{
    public class PlayerFactory : IPlayerFactory
    {
        public readonly ITgsRepository _repository;

        public PlayerFactory(ITgsRepository repository)
        {
            _repository = repository;
        }

        public async ValueTask<(PlayerAccountChangeModel[], int)> GetPlayerAccountChangeAsync(
            GetPlayerAccountChangesRequest request)
        {
            const string spName = "dbo.report_get_player_account_change_records";

            TransactionType? transactionType = request.AccountChangeType switch
            {
                AccountChangeType.Deposit => TransactionType.Deposit,
                AccountChangeType.Withdrawal => TransactionType.Withdrawal,
                AccountChangeType.Adjust => TransactionType.Adjust,
                AccountChangeType.Bonus => TransactionType.Bonus,
                AccountChangeType.Rebate => TransactionType.Rebate,
                AccountChangeType.Dividend => TransactionType.Dividend,
                AccountChangeType.Promotion => TransactionType.Promotion,
                AccountChangeType.GameProviderFundOut => TransactionType.GameProviderFundOut,
                AccountChangeType.GameProviderFundIn => TransactionType.GameProviderFundIn,
                AccountChangeType.CancelWithdrawal => TransactionType.CancelWithdrawal,
                AccountChangeType.GameProviderDeposit => TransactionType.GameProviderDeposit,
                AccountChangeType.GameProviderWithdrawal => TransactionType.GameProviderWithdrawal,
                AccountChangeType.UnsettledBetManual => TransactionType.UnsettledBetManual,
                AccountChangeType.DividendAllAgentRebate => TransactionType.DividendAllAgentRebate,
                _ => null
            };

            GameActionType? gameActionType = request.AccountChangeType switch
            {
                AccountChangeType.PlaceBet => GameActionType.PlaceBet,
                AccountChangeType.WinBet => GameActionType.WinBet,
                AccountChangeType.LoseBet => GameActionType.LoseBet,
                AccountChangeType.FreeBet => GameActionType.FreeBet,
                AccountChangeType.TieBet => GameActionType.TieBet,
                AccountChangeType.CancelTransaction => GameActionType.CancelTransaction,
                AccountChangeType.LockAmount => GameActionType.LockAmount,
                AccountChangeType.UnlockAmount => GameActionType.UnlockAmount,
                AccountChangeType.WinJackpot => GameActionType.WinJackpot,
                _ => null
            };

            var conn = _repository.GetDbConnection();
            var transaction = _repository.CurrentTransaction?.GetDbTransaction();
            var results = await conn.QueryAsync<PlayerAccountChangeModel>(spName, new Dictionary<string, object>
            {
                { "_from", request.From },
                { "_to", request.To },
                { "_player_code", request.PlayerCode },
                { "_transaction_type", transactionType },
                { "_game_action_type", gameActionType },
                { "_page_size", request.ShowAll ? int.MaxValue : request.Rows },
                { "_page_number", request.ShowAll ? 1 : request.Page }
            }, commandType: CommandType.StoredProcedure, transaction: transaction);

            return (results.ToArray(), results.Any() ? results.First().TotalCount : 0);
            ;
        }

        public ValueTask<decimal> GetPlayerAccountChangeTotalAmountAsync(GetPlayerAccountChangesTotalAmountRequest request)
        {
            TransactionType? transactionType = request.AccountChangeType switch
            {
                AccountChangeType.Deposit => TransactionType.Deposit,
                AccountChangeType.Withdrawal => TransactionType.Withdrawal,
                AccountChangeType.Adjust => TransactionType.Adjust,
                AccountChangeType.Bonus => TransactionType.Bonus,
                AccountChangeType.Rebate => TransactionType.Rebate,
                AccountChangeType.Dividend => TransactionType.Dividend,
                AccountChangeType.Promotion => TransactionType.Promotion,
                AccountChangeType.GameProviderFundOut => TransactionType.GameProviderFundOut,
                AccountChangeType.GameProviderFundIn => TransactionType.GameProviderFundIn,
                AccountChangeType.CancelWithdrawal => TransactionType.CancelWithdrawal,
                AccountChangeType.GameProviderDeposit => TransactionType.GameProviderDeposit,
                AccountChangeType.GameProviderWithdrawal => TransactionType.GameProviderWithdrawal,
                AccountChangeType.UnsettledBetManual => TransactionType.UnsettledBetManual,
                AccountChangeType.DividendAllAgentRebate => TransactionType.DividendAllAgentRebate,
                _ => null
            };

            GameActionType? gameActionType = request.AccountChangeType switch
            {
                AccountChangeType.PlaceBet => GameActionType.PlaceBet,
                AccountChangeType.WinBet => GameActionType.WinBet,
                AccountChangeType.LoseBet => GameActionType.LoseBet,
                AccountChangeType.FreeBet => GameActionType.FreeBet,
                AccountChangeType.TieBet => GameActionType.TieBet,
                AccountChangeType.CancelTransaction => GameActionType.CancelTransaction,
                AccountChangeType.LockAmount => GameActionType.LockAmount,
                AccountChangeType.UnlockAmount => GameActionType.UnlockAmount,
                AccountChangeType.WinJackpot => GameActionType.WinJackpot,
                _ => null
            };

            var transType = (short?)transactionType;
            var gameType = (short?)gameActionType;

            var queryTransaction =
                from t in _repository.Transactions
                where t.RecordedOn >= request.From
                    && t.RecordedOn < request.To
                    && t.PlayerCode == request.PlayerCode
                select new { t.Amount, t.Type };

            if (gameType is null && transType is not null)
                queryTransaction = queryTransaction.Where(t => t.Type == transType);

            var queryGameAction =
                from ga in _repository.GameActions
                where ga.RecordedOn >= request.From
                    && ga.RecordedOn < request.To
                    && ga.PlayerCode == request.PlayerCode
                select new { ga.Amount, ga.Type };

            if (transType is null && gameType is not null)
                queryGameAction = queryGameAction.Where(t => t.Type == gameType);

            var transactionGameActionUnionAll = queryTransaction.Concat(queryGameAction);

            var totalAmount = transactionGameActionUnionAll.Sum(tgu => tgu.Amount);

            return ValueTask.FromResult(totalAmount);
        }
    }
}