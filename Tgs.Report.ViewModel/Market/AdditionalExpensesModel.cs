using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Market
{
    public record AdditionalExpensesModel
    {
        public AdditionalExpensesType AdditionalExpensesType { get; init; }

        public decimal Amount { get; init; }
    }
}