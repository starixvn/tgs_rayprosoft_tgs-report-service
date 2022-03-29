#nullable enable
using System;
using Tgs.Report.Enum;

namespace Tgs.Report.Repository.Configurations
{
    public class Round
    {
        public Guid Id { get; set; }
        public string ExternalRoundId { get; set; }
        public Guid GameProviderId { get; set; }
        public string PlayerCode { get; set; }
        public int GameId { get; set; }
        public string CurrencyCode { get; set; }
        public RoundStatus Status { get; set; }
        public Guid GameProviderConfigurationId { get; set; }
        public Guid WalletId { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset? ClosedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
        public Guid BrandId { get; set; }
        public DateTimeOffset? ExternalCreatedOn { get; set; }
        public DateTimeOffset? ExternalClosedOn { get; set; }
        public decimal? BetAmount { get; set; }
        public decimal? ValidBetAmount { get; set; }
        public decimal? SettleAmount { get; set; }
        public decimal? Commission { get; set; }
        public decimal? Turnover { get; set; }
        public decimal? WinLoss { get; set; }
        public decimal? TieBetAdjustment { get; set; }
        public ClientRoundStatus ClientStatus { get; set; }
        public decimal? ReopenTurnoverAdjustment { get; set; }
        public DateTimeOffset? ReopenedOn { get; set; }
        public decimal? EffectiveBettingPercentage { get; set; }
        public virtual RoundDetail RoundDetail { get; set; }
    }
}