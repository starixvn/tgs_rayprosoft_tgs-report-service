using System.Collections.Generic;
using System.Text.Json.Serialization;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Round.BetHistories
{
    public record TableData : BaseData
    {
        [JsonPropertyName("headers")]
        public string[] Headers { get; init; }

        [JsonPropertyName("rows")]
        public List<Dictionary<string, string>> Rows { get; init; }

        public TableData() : base(BetHistoryDataType.Table)
        {
        }
    }
}