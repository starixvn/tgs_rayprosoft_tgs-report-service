namespace Tgs.Report.ViewModel.Market
{
    public record GetAdditionalExpensesResponse
    {
        public AdditionalExpensesModel[] AdditionalExpenses { get; init; }
    }
}