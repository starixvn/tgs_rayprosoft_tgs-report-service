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

        public PlayerFactory(ITgsRepository repository) => _repository = repository;

        public async ValueTask<(PlayerAccountChangeModel[], int)> GetPlayerAccountChangeAsync(
            GetPlayerAccountChangesRequest request)
        {
            const string spName = "dbo.report_get_player_account_change_records";

            TransactionType? transactionType = request.AccountChangeType switch
            {
                AccountChangeType.Deposit => TransactionType.Deposit,
                AccountChangeType.Withdrawal => TransactionType.Withdrawal,
                AccountChangeType.CompanyAdjust => TransactionType.CompanyAdjust,
                AccountChangeType.BonusAdjust => TransactionType.BonusAdjust,
                AccountChangeType.AgentAdjust => TransactionType.AgentAdjust,
                AccountChangeType.SystemAdjust => TransactionType.SystemAdjust,
                AccountChangeType.TransferAdjust => TransactionType.TransferAdjust,
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
                AccountChangeType.PromotionVipLevelUpgrade => TransactionType.PromotionVipLevelUpgrade,
                AccountChangeType.PromotionVipLevelSeason => TransactionType.PromotionVipLevelSeason,
                AccountChangeType.PromotionVipLevelBirthday => TransactionType.PromotionVipLevelBirthday,
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
            var playerAccountChangeModels = await conn.QueryAsync<PlayerAccountChangeModel>(spName,
                new Dictionary<string, object?>
                {
                    { "_from", request.From },
                    { "_to", request.To },
                    { "_player_code", request.PlayerCode },
                    { "_transaction_type", transactionType },
                    { "_game_action_type", gameActionType },
                    { "_page_size", request.ShowAll ? int.MaxValue : request.Rows },
                    { "_page_number", request.ShowAll ? 1 : request.Page }
                }, commandType: CommandType.StoredProcedure, transaction: transaction);

            var playerAccountChangeResult = playerAccountChangeModels.ToArray();

            var transactionIds = playerAccountChangeResult
                .Select(pac => pac.TransactionId)
                .ToArray();

            var adjustPlayerRecords = _repository.AdjustPlayerRecords
                .Where(apr => transactionIds.Contains(apr.TransactionId))
                .ToArray();

            var result = from pac in playerAccountChangeResult
                         join apr in adjustPlayerRecords on pac.TransactionId equals apr.TransactionId into tmpPAC
                         from apr in tmpPAC.DefaultIfEmpty()
                         select new PlayerAccountChangeModel
                         {
                             Amount = pac.Amount,
                             Balance = pac.Balance,
                             Description = pac.Description,
                             GameActionId = pac.GameActionId,
                             GameActionType = pac.GameActionType,
                             IncludeInNetProfit = apr != null ? apr.IncludeInNetProfit : false,
                             RecordedOn = pac.RecordedOn,
                             TotalCount = pac.TotalCount,
                             TransactionId = pac.TransactionId,
                             TransactionType = pac.TransactionType
                         };

            return (result.ToArray(), result.Any() ? result.First().TotalCount : 0);
        }

        public ValueTask<decimal> GetPlayerAccountChangeTotalAmountAsync(
            GetPlayerAccountChangesTotalAmountRequest request)
        {
            TransactionType? transactionType = request.AccountChangeType switch
            {
                AccountChangeType.Deposit => TransactionType.Deposit,
                AccountChangeType.Withdrawal => TransactionType.Withdrawal,
                AccountChangeType.CompanyAdjust => TransactionType.CompanyAdjust,
                AccountChangeType.BonusAdjust => TransactionType.BonusAdjust,
                AccountChangeType.AgentAdjust => TransactionType.AgentAdjust,
                AccountChangeType.SystemAdjust => TransactionType.SystemAdjust,
                AccountChangeType.TransferAdjust => TransactionType.TransferAdjust,
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
                AccountChangeType.PromotionVipLevelUpgrade => TransactionType.PromotionVipLevelUpgrade,
                AccountChangeType.PromotionVipLevelSeason => TransactionType.PromotionVipLevelSeason,
                AccountChangeType.PromotionVipLevelBirthday => TransactionType.PromotionVipLevelBirthday,
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
                    && t.PlayerCode == request.PlayerCode && (transType == null || t.Type == transType) &&
                    gameType == null
                select new
                {
                    t.Amount,
                    t.Type
                };

            var queryGameAction =
                from ga in _repository.GameActions
                where ga.RecordedOn >= request.From
                    && ga.RecordedOn < request.To
                    && ga.PlayerCode == request.PlayerCode && (gameType == null || ga.Type == gameType) &&
                    transType == null
                select new
                {
                    ga.Amount,
                    ga.Type
                };

            var transactionGameActionUnionAll = queryTransaction.Concat(queryGameAction);

            var totalAmount = transactionGameActionUnionAll.Sum(tgu => tgu.Amount);

            return ValueTask.FromResult(totalAmount);
        }
    }
}