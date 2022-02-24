namespace Tgs.Report.Client.Models.Market
{
    public record GetAdditionalExpensesResponse
    {
        public AdditionalExpensesModel[] AdditionalExpenses { get; init; }
    }
}