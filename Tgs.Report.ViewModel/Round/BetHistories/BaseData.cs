using System.Text.Json.Serialization;
using Tgs.Report.Enum;
using Tgs.Report.ViewModel.Round.BetHistories.JsonConverters;

namespace Tgs.Report.ViewModel.Round.BetHistories
{
    [JsonConverter(typeof(DataConverter))]
    public abstract record BaseData
    {
        [JsonPropertyName("type")]
        public BetHistoryDataType Type { get; }

        protected BaseData(BetHistoryDataType type)
        {
            Type = type;
        }
    }
}