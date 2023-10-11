using Tgs.Report.Client.Enum;

namespace Tgs.Report.Client.Models.Player
{
    public class PlayerAllAgentRebateDetailGameTypeModel
    {
        public AllAgentGameType AllAgentGameType { get; set; }
        public int ActivePlayerNumber { get; set; }
        public decimal RebatePercentage { get; set; }
    }
}
