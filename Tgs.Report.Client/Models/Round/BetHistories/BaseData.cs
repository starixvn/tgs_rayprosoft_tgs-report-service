using System.Text.Json.Serialization;
using Tgs.Report.Client.Models.Round.BetHistories.JsonConverters;

namespace Tgs.Report.Client.Models.Round.BetHistories
{
    [JsonConverter(typeof(DataConverter))]
    public abstract record BaseData
    {
        [JsonPropertyName("type")]
        public short Type { get; }

        protected BaseData(short type)
        {
            Type = type;
        }
    }
}