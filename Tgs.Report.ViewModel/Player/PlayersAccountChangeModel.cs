using System;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Player
{
    public class PlayersAccountChangeModel
    {
        public string Username { get; set; }
        public decimal Amount { get; set; }
        public decimal BeforeBalance { get; set; }
        public decimal AfterBalance { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTimeOffset RecordedOn { get; set; }
        public string Operator { get; set; }
        public string Description { get; set; }
    }
}