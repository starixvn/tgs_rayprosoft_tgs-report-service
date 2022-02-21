using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Round.BetHistories
{
    public sealed record JsonData : ValueData
    {
        public JsonData() : base(BetHistoryDataType.Json)
        {
        }
    }
}