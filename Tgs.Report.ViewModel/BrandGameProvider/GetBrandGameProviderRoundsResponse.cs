namespace Tgs.Report.ViewModel.BrandGameProvider
{
    public record GetBrandGameProviderRoundsResponse
    {
        public PagedResponse<BrandGameProviderRoundsModel>? Datas { get; init; }

        public BrandGameProviderRoundTotalModel[]? Totals { get; init; }
    }
}