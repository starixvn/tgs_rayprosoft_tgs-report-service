using System;

namespace Tgs.Report.Client.Models.GameAction
{
    public record GameActionModel
    {
        public DateTimeOffset RecordedOn { get; init; }

        public string ExternalTransactionId { get; init; }

        public short GameActionType { get; init; }

        public decimal Amount { get; init; }

        public decimal BeforeBalance { get; init; }

        public decimal AfterBalance { get; init; }
    }
}