using TGS.Core.Infrastructures.Extensions.Models;

namespace Tgs.Report.ViewModel.BrandGameProvider;

public record GetBrandGameProvidersResponse
{
    public PagedResponse<BrandGameProvidersModel> Datas { get; init; } = default!;

    public BrandGameProviderTotalModel Total { get; init; } = default!;
}