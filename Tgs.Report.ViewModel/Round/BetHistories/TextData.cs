using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Round.BetHistories
{
    public sealed record TextData : ValueData
    {
        public TextData() : base(BetHistoryDataType.Text)
        {
        }
    }
}