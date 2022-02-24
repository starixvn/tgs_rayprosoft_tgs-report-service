using System.Text.Json.Serialization;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Round.BetHistories
{
    public sealed record UrlData : BaseData
    {
        [JsonPropertyName("url")]
        public string Url { get; init; }

        public UrlData() : base(BetHistoryDataType.Url)
        {
        }
    }
}