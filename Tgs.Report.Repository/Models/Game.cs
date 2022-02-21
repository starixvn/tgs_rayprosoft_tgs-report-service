using System;
using Tgs.Report.Enum;

namespace Tgs.Report.Repository.Models
{
    public class Game : BaseEntity<int>
    {
        public GameStatus Status { get; set; }

        public string ExternalId { get; set; }

        public string Description { get; set; }

        public string EndpointPath { get; set; }

        public string RuleUrl { get; set; }

        public Guid GameProviderId { get; set; }

        public virtual GameProvider GameProvider { get; set; }

        public GameType Type { get; set; }

        public GamePlatformType PlatformType { get; set; }

        public bool SubGame { get; set; }

        public bool AvailableToBrand { get; set; }

        public bool HasTurnoverOnTieBet { get; set; }

        public bool Html5 { get; set; }

        /// <summary>
        ///     Currencies that the game supports, recorded by csv format
        ///     Empty for all currencies that game provider has.
        /// </summary>
        public string AvailableCurrencies { get; set; }

        public string Localization { get; set; }

        public string SubGameProviderCode { get; set; }
        public virtual SubGameProvider SubGameProvider { get; set; }
    }
}