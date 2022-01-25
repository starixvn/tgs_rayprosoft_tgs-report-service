using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tgs.Report.Client.Models.Round.BetHistories
{
    public record TableData : BaseData
    {
        [JsonPropertyName("headers")]
        public string[] Headers { get; init; }

        [JsonPropertyName("rows")]
        public List<Dictionary<string, string>> Rows { get; init; }

        public TableData() : base(2)
        {
        }
    }
}