using Tgs.Report.Enum;

namespace Tgs.Report.Client.Models.Market
{
    public record AdditionalExpensesModel
    {
        public AdditionalExpensesType AdditionalExpensesType { get; init; }

        public decimal Amount { get; init; }
    }
}