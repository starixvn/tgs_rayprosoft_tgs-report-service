using Tgs.Report.Enum;

namespace Tgs.Report.Client.Models.Market
{
    public class AdditionalExpensesModel
    {
        public AdditionalExpensesType AdditionalExpensesType { get; set; }

        public decimal Amount { get; set; }
    }
}