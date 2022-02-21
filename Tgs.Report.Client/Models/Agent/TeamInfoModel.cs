using System;

namespace Tgs.Report.Client.Models.Agent
{
    public record TeamInfoModel
    {
        public string Username { get; init; }
        public DateTimeOffset RegisteredOn { get; init; }
        public decimal Contribution { get; init; }
    }
}