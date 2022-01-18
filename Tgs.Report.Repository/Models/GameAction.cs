using System;
using Tgs.Report.Enum;

namespace Tgs.Report.Repository.Models
{
    public class GameAction
    {
        public Guid Id { get; set; }
        public Guid RoundId { get; set; }
        public virtual Round Round { get; set; }
        public string PlayerCode { get; set; }
        public string ExternalTransactionId { get; set; }
        public GameActionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal Turnover { get; set; }
        public decimal WinLoss { get; set; }
        public decimal UnsettledBets { get; set; }
        public decimal Balance { get; set; }
        public string BetType { get; set; }
        public string ExternalBetId { get; set; }
        public string ExternalGameActionGroupId { get; set; }
        public Guid? ReferencedGameActionId { get; set; }
        public Guid? GameActionBatchId { get; set; }
        public DateTimeOffset RecordedOn { get; set; }
        public DateTimeOffset? SentOn { get; set; }
        public bool IsClosingRound { get; set; }
        public bool? ClosingRoundExplicitRequest { get; set; }
        public string PlayerIp { get; set; }
        public string ExtraData { get; set; }
        public string JackpotContribution { get; init; }
    }
}