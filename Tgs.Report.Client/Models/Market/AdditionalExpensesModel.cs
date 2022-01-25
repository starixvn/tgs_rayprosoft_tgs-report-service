namespace Tgs.Report.Client.Models.Market
{
    public record AdditionalExpensesModel
    {
        public short AdditionalExpensesType { get; init; }

        public decimal Amount { get; init; }
    }
}