namespace Tgs.Report.ViewModel.BrandGameProvider;

public record GetBrandGameProvidersResponse
{
    public PagedResponse<BrandGameProvidersModel>? Datas { get; init; }

    public BrandGameProviderTotalModel[]? Totals { get; init; }
}