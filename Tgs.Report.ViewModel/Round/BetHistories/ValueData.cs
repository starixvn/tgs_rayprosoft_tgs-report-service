using System.Text.Json.Serialization;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Round.BetHistories
{
    public abstract record ValueData : BaseData
    {
        [JsonPropertyName("value")]
        public string Value { get; init; }

        protected ValueData(BetHistoryDataType type) : base(type)
        {
        }
    }
}