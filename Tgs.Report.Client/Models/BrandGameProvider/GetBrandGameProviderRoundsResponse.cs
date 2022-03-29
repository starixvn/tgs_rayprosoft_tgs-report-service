namespace Tgs.Report.Client.Models.BrandGameProvider
{
    public record GetBrandGameProviderRoundsResponse
    {
        public PagedResponse<BrandGameProviderRoundsModel> Datas { get; init; } = default!;

        public BrandGameProviderRoundTotalModel[] Totals { get; init; } = default!;
    }
}