using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Market;

namespace Tgs.Report.Core.Queries.Market;

public class GetMarketsCacheHandler(
    IMemoryCache memoryCache,
    IRequestHandler<GetMarketsRequest, PagedResponse<MarketModel>> handler)
    : IRequestHandler<GetMarketsRequest, PagedResponse<MarketModel>>
{
    public async Task<PagedResponse<MarketModel>> Handle(
        GetMarketsRequest request,
        CancellationToken cancellationToken)
    {
        var offset = new TimeSpan(-4, 0, 0);
        var requestTo = request.To.ToOffset(offset);

        var to = new DateTimeOffset(requestTo.Year,
            requestTo.Month,
            requestTo.Day,
            0,
            0,
            0,
            requestTo.Offset);

        var now = DateTimeOffset.Now.ToOffset(offset);
        var today = new DateTimeOffset(now.Year,
            now.Month,
            now.Day,
            0,
            0,
            0,
            requestTo.Offset);

        if (to > today)
        {
            return await handler
                .Handle(request, cancellationToken)
                .ConfigureAwait(false);
        }

        var key =
            $"MarketsReport:{request.BrandId}:{request.From}:{request.To}:{request.Page}:{request.Rows}:{request.OrderBy}:{request.SortBy}:{request.ShowAll}";

        return await memoryCache.GetOrCreateAsync(
            key,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5D);

                return await handler
                    .Handle(request, cancellationToken)
                    .ConfigureAwait(false);
            }).ConfigureAwait(false) ?? new PagedResponse<MarketModel>();
    }
}