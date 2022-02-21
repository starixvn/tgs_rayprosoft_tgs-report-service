using System.Text.Json.Serialization;

namespace Tgs.Report.Client.Models.Round.BetHistories
{
    public abstract record ValueData : BaseData
    {
        [JsonPropertyName("value")]
        public string Value { get; init; }

        protected ValueData(short type) : base(type)
        {
        }
    }
}