using MediatR;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Infrastructures.Extensions.Models;
using Tgs.Report.Core.Models;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player;

public class
    GetPlayerAccountChangeQueryHandler(IPlayerFactory playerFactory) : IRequestHandler<GetPlayerAccountChangesRequest,
    PagedResponse<PlayerAccountChangeModel>>
{
    public async Task<PagedResponse<PlayerAccountChangeModel>> Handle(
        GetPlayerAccountChangesRequest request,
        CancellationToken cancellationToken)
    {
        var (playerAccountChanges, totalCount) = await playerFactory.GetPlayerAccountChangeAsync(new GetPlayerAccountChangesQuery
        {
            BrandId = request.BrandId,
            PlayerCode = request.PlayerCode,
            AccountChangeType = request.AccountChangeType,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        });

        return await playerAccountChanges.CreatePagedResponseAsync(totalCount);
    }
}