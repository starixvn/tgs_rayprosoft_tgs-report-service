using MediatR;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Market;

namespace Tgs.Report.Core.Queries.Market;

public class GetPlayerDepositFirstDetailQueryHandler : IRequestHandler<GetPlayerFirstDepositDetailsRequest,
    PagedResponse<PlayerFirstDepositDetailModel>>
{
    private readonly IPlayerDepositFactory _playerDepositFactory;

    public GetPlayerDepositFirstDetailQueryHandler(IPlayerDepositFactory playerDepositFactory)
        => _playerDepositFactory = playerDepositFactory;

    public async Task<PagedResponse<PlayerFirstDepositDetailModel>> Handle(
        GetPlayerFirstDepositDetailsRequest request,
        CancellationToken cancellationToken)
    {
        var offset = new TimeSpan(8, 0, 0);
        var requestFrom = request.Date.ToOffset(offset);
        var requestTo = request.Date.ToOffset(offset).AddDays(1);

        var from = new DateTimeOffset(requestFrom.Year,
            requestFrom.Month,
            requestFrom.Day,
            0,
            0,
            0,
            requestFrom.Offset);

        var to = new DateTimeOffset(requestTo.Year,
            requestTo.Month,
            requestTo.Day,
            0,
            0,
            0,
            requestTo.Offset);

        var details = await _playerDepositFactory.GetPlayerFirstDepositDetailsAsync(request.BrandId, from.ToUniversalTime(), to.ToUniversalTime());

        return await details.CreatePagedResponseAsync(details.Count());
    }
}