using System.Text.Json.Serialization;

namespace Tgs.Report.Client.Models.Round.BetHistories
{
    public sealed record UrlData : BaseData
    {
        [JsonPropertyName("url")]
        public string Url { get; init; }

        public UrlData() : base(1)
        {
        }
    }
}