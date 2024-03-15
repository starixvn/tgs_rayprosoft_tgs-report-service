namespace Tgs.Report.Client.Models.BrandGameProvider;

public record GetBrandGameProvidersResponse
{
    public PagedResponse<BrandGameProvidersModel>? Datas { get; init; }

    public BrandGameProviderTotalModel[]? Totals { get; init; }
}