using System;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Round
{
    public class OpenRoundModel
    {
        public Guid Id { get; set; }

        public string ExternalRoundId { get; set; }

        public string BrandName { get; set; }

        public string BrandCode { get; set; }

        public int GameId { get; set; }

        public string GameName { get; set; }

        public Guid GameProviderId { get; set; }

        public string GameProviderCode { get; set; }

        public string GameProviderName { get; set; }

        public string SubGameProviderName { get; set; }

        public string PlayerName { get; set; }

        public string PlayerCode { get; set; }

        public string ExternalPlayerCode { get; set; }

        public string CurrencyCode { get; set; }

        public PlatformType PlatformType { get; set; }

        public ClientRoundStatus Status { get; set; }

        public PlayerType PlayerType { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset? ClosedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }

        public DateTimeOffset? ExternalCreatedOn { get; set; }

        public DateTimeOffset? ExternalClosedOn { get; set; }

        public decimal? BetAmount { get; set; }

        public decimal? ValidBetAmount { get; set; }

        public decimal? SettleAmount { get; set; }

        public decimal? Commission { get; set; }

        public decimal? WinLoss { get; set; }

        public decimal? TieBetAdjustment { get; set; }

        public string AdditionalData { get; set; }
    }
}