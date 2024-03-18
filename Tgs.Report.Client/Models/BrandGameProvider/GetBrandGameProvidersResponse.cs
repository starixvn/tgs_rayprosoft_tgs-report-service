namespace Tgs.Report.Client.Models.BrandGameProvider;

public record GetBrandGameProvidersResponse
{
    public PagedResponse<BrandGameProvidersModel> Datas { get; init; } = default!;

    public BrandGameProviderTotalModel Total { get; init; } = default!;
}