using System;
using System.Collections.Generic;
using Tgs.Report.Enum;

namespace Tgs.Report.Repository.Models
{
    public class GameProvider : BaseEntity<Guid>
    {
        public Status Status { get; set; }
        public string Description { get; set; }
        public bool AutoCloseRound { get; set; }
        public bool SupportTestPlayer { get; set; }
        public bool AllowKickPlayer { get; set; }
        public int PlaceBetTimeout { get; set; }
        public int SettleBetTimeout { get; set; }
        public ushort UniqueId { get; set; }
        public bool TransferWallet { get; set; }
        public string Localization { get; set; }
        public virtual ICollection<Game> Games { get; set; }
        public virtual ICollection<SubGameProvider> SubGameProviders { get; set; }
    }
}