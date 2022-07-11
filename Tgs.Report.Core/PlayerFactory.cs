using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core
{
    public class PlayerFactory : IPlayerFactory
    {
        private readonly TgsContext _context;

        public PlayerFactory(TgsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

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
                AccountChangeType.GameDebit => TransactionType.GameDebit,
                AccountChangeType.GameCredit => TransactionType.GameCredit,
                AccountChangeType.GameRefund => TransactionType.GameRefund,
                AccountChangeType.GameRevision => TransactionType.GameRevision,
                AccountChangeType.GameBet => TransactionType.GameBet,
                AccountChangeType.GameSettle => TransactionType.GameSettle,
                AccountChangeType.GameSettleWin => TransactionType.GameSettleWin,
                AccountChangeType.GameSettleLose => TransactionType.GameSettleLose,
                AccountChangeType.GameSettleFree => TransactionType.GameSettleFree,
                AccountChangeType.GameSettleTie => TransactionType.GameSettleTie,
                AccountChangeType.GameWinJackpot => TransactionType.GameWinJackpot,
                AccountChangeType.GameCancel => TransactionType.GameCancel,
                AccountChangeType.GameLock => TransactionType.GameLock,
                AccountChangeType.GameUnlock => TransactionType.GameUnlock,
                AccountChangeType.GameProviderFundRefund => TransactionType.GameProviderFundRefund,
                _ => null
            };

            GameActionType? gameActionType = request.AccountChangeType switch
            {
                AccountChangeType.GameBet => GameActionType.PlaceBet,
                AccountChangeType.GameSettleWin => GameActionType.WinBet,
                AccountChangeType.GameSettleLose => GameActionType.LoseBet,
                AccountChangeType.GameSettleFree => GameActionType.FreeBet,
                AccountChangeType.GameSettleTie => GameActionType.TieBet,
                AccountChangeType.GameCancel => GameActionType.CancelTransaction,
                AccountChangeType.GameLock => GameActionType.LockAmount,
                AccountChangeType.GameUnlock => GameActionType.UnlockAmount,
                AccountChangeType.GameWinJackpot => GameActionType.WinJackpot,
                _ => null
            };

            var conn = _context.Database.GetDbConnection();
            var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
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

            var adjustPlayerRecords = _context.AdjustPlayerRecords
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
                             IncludeInNetProfit = apr?.IncludeInNetProfit ?? false,
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
            var transactionTypes = request.AccountChangeType switch
            {
                AccountChangeType.Deposit => new[] { TransactionType.Deposit, TransactionType.Deposited },
                AccountChangeType.Withdrawal => new[] { TransactionType.Withdrawal, TransactionType.WithdrawLock },
                AccountChangeType.CompanyAdjust => new[] { TransactionType.CompanyAdjust },
                AccountChangeType.BonusAdjust => new[] { TransactionType.BonusAdjust },
                AccountChangeType.AgentAdjust => new[] { TransactionType.AgentAdjust },
                AccountChangeType.SystemAdjust => new[] { TransactionType.SystemAdjust },
                AccountChangeType.TransferAdjust => new[] { TransactionType.TransferAdjust },
                AccountChangeType.Bonus => new[] { TransactionType.Bonus },
                AccountChangeType.Rebate => new[] { TransactionType.Rebate },
                AccountChangeType.Dividend => new[] { TransactionType.Dividend },
                AccountChangeType.Promotion => new[] { TransactionType.Promotion },
                AccountChangeType.GameProviderFundOut => new[] { TransactionType.GameProviderFundOut },
                AccountChangeType.GameProviderFundIn => new[] { TransactionType.GameProviderFundIn },
                AccountChangeType.CancelWithdrawal => new[] { TransactionType.CancelWithdrawal, TransactionType.WithdrawRollback },
                AccountChangeType.GameProviderDeposit => new[] { TransactionType.GameProviderDeposit },
                AccountChangeType.GameProviderWithdrawal => new[] { TransactionType.GameProviderWithdrawal },
                AccountChangeType.UnsettledBetManual => new[] { TransactionType.UnsettledBetManual },
                AccountChangeType.DividendAllAgentRebate => new[] { TransactionType.DividendAllAgentRebate },
                AccountChangeType.PromotionVipLevelUpgrade => new[] { TransactionType.PromotionVipLevelUpgrade },
                AccountChangeType.PromotionVipLevelSeason => new[] { TransactionType.PromotionVipLevelSeason },
                AccountChangeType.PromotionVipLevelBirthday => new[] { TransactionType.PromotionVipLevelBirthday },
                AccountChangeType.GameDebit => new[] { TransactionType.GameDebit },
                AccountChangeType.GameCredit => new[] { TransactionType.GameCredit },
                AccountChangeType.GameRefund => new[] { TransactionType.GameRefund },
                AccountChangeType.GameRevision => new[] { TransactionType.GameRevision },
                AccountChangeType.GameBet => new[] { TransactionType.GameBet },
                AccountChangeType.GameSettle => new[] { TransactionType.GameSettle },
                AccountChangeType.GameSettleWin => new[] { TransactionType.GameSettleWin },
                AccountChangeType.GameSettleLose => new[] { TransactionType.GameSettleLose },
                AccountChangeType.GameSettleFree => new[] { TransactionType.GameSettleFree },
                AccountChangeType.GameSettleTie => new[] { TransactionType.GameSettleTie },
                AccountChangeType.GameWinJackpot => new[] { TransactionType.GameWinJackpot },
                AccountChangeType.GameCancel => new[] { TransactionType.GameCancel },
                AccountChangeType.GameLock => new[] { TransactionType.GameLock },
                AccountChangeType.GameUnlock => new[] { TransactionType.GameUnlock },
                AccountChangeType.GameProviderFundRefund => new[] { TransactionType.GameProviderFundRefund },
                _ => null
            };

            GameActionType? gameActionType = request.AccountChangeType switch
            {
                AccountChangeType.GameBet => GameActionType.PlaceBet,
                AccountChangeType.GameSettleWin => GameActionType.WinBet,
                AccountChangeType.GameSettleLose => GameActionType.LoseBet,
                AccountChangeType.GameSettleFree => GameActionType.FreeBet,
                AccountChangeType.GameSettleTie => GameActionType.TieBet,
                AccountChangeType.GameCancel => GameActionType.CancelTransaction,
                AccountChangeType.GameLock => GameActionType.LockAmount,
                AccountChangeType.GameUnlock => GameActionType.UnlockAmount,
                AccountChangeType.GameWinJackpot => GameActionType.WinJackpot,
                _ => null
            };

            var transTypes = transactionTypes?.Select(x => (short?)x);
            var gameType = (short?)gameActionType;

            var queryTransaction =
                from t in _context.Transactions
                where t.RecordedOn >= request.From
                    && t.RecordedOn < request.To
                    && t.PlayerCode == request.PlayerCode && (transTypes == null || transTypes.Contains(t.Type)) &&
                    gameType == null
                select new
                {
                    t.Amount,
                    t.Type
                };

            var queryGameAction =
                from ga in _context.GameActions
                where ga.RecordedOn >= request.From
                    && ga.RecordedOn < request.To
                    && ga.PlayerCode == request.PlayerCode && (gameType == null || ga.Type == gameType) &&
                    transTypes == null
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