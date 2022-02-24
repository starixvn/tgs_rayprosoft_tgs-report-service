using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Round.BetHistories
{
    public sealed record XmlData : ValueData
    {
        public XmlData() : base(BetHistoryDataType.Xml)
        {
        }
    }
}