using MediatR;
using TGS.Core.Infrastructures.Extensions;
using Tgs.Report.Core.Models;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player;

internal class
    GetPlayerAllAgentRebatesQueryHandle(IPlayerFactory playerFactory)
    : IStreamRequestHandler<GetPlayerAllAgentRebatesRequest,
        PlayerAllAgentRebateModel>
{
    public IAsyncEnumerable<PlayerAllAgentRebateModel> Handle(GetPlayerAllAgentRebatesRequest request,
        CancellationToken cancellationToken)
    {
        var offset = new TimeSpan(8, 0, 0);
        var requestFrom = new DateTimeOffset(request.From, TimeSpan.Zero).ToOffset(offset);
        var requestTo = new DateTimeOffset(request.To, TimeSpan.Zero).ToOffset(offset);

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

        return playerFactory
            .GetPlayerAllAgentRebatesQueryable(new GetPlayerAllAgentRebatesQuery(request.BrandId,
                from.UtcDateTime,
                to.UtcDateTime,
                request.Username,
                request.PlayerCode))
            .CreateResponseAsyncEnumerable(request);
    }
}