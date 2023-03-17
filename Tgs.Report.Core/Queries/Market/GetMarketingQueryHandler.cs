using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Marketing;

namespace Tgs.Report.Core.Queries.Market;

public class GetMarketingQueryHandler : IRequestHandler<GetMarketingListRequest, PagedResponse<MarketingModel>>
{
    private readonly TgsContext _context;

    public GetMarketingQueryHandler(TgsContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<PagedResponse<MarketingModel>> Handle(
        GetMarketingListRequest request,
        CancellationToken cancellationToken)
    {
        var offset = new TimeSpan(8, 0, 0);
        var requestFrom = request.From.ToOffset(offset);
        var requestTo = request.To.ToOffset(offset);

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

        var agent = await _context.Agents
            .FirstOrDefaultAsync(x => x.BrandId == request.BrandId && x.Username == request.AgentUsername,
                cancellationToken).ConfigureAwait(false);

        if (agent == null)
        {
            return new()
            {
                Valid = true,
                Items = Array.Empty<MarketingModel>()
            };
        }

        var dates = Enumerable.Range(0, to.Subtract(from).Days)
            .Select(x => from.AddDays(x))
            .ToArray();

        var playerAggregates = await _context.PlayerAggregates
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.AgentId == agent.Id)
            .Where(x => x.ReportDate >= from.ToUniversalTime() && x.ReportDate < to.ToUniversalTime())
            .GroupBy(x => new
            {
                x.ReportDate,
                x.PlayerCode
            }, (player, group) => new
            {
                player.ReportDate,
                player.PlayerCode,
                RegisterCount = group.Sum(x => x.RegisteredCount),
                FirstDepositCount = group.Sum(x => x.FirstFundInCount),
                BetAmount = group.Sum(x => x.BetAmount)
            }).ToArrayAsync(cancellationToken).ConfigureAwait(false);

        var dateAggregates = playerAggregates.GroupBy(x => x.ReportDate,
            (reportDate, group) => new MarketingModel
            {
                ReportDate = reportDate,
                RegisterCount = group.Sum(x => x.RegisterCount),
                FirstDepositCount = group.Sum(x => x.FirstDepositCount),
                GamingCount = group.Count(x => x.BetAmount > 0)
            });

        return dates.GroupJoin(dateAggregates,
                date => date,
                player => player.ReportDate,
                (date, groupDeposits) => new
                {
                    date,
                    groupDeposits
                })
            .SelectMany(x => x.groupDeposits.DefaultIfEmpty(),
                (x, player) => new
                {
                    ReportDate = x.date,
                    player
                })
            .GroupBy(x => new { x.ReportDate },
                (date, groupDeposits) => new MarketingModel
                {
                    ReportDate = date.ReportDate,
                    RegisterCount =
                        groupDeposits.FirstOrDefault(x => date.ReportDate == x.ReportDate)?.player?.RegisterCount ?? 0,
                    FirstDepositCount =
                        groupDeposits.FirstOrDefault(x => date.ReportDate == x.ReportDate)?.player?.FirstDepositCount ??
                        0,
                    GamingCount = groupDeposits.FirstOrDefault(x => date.ReportDate == x.ReportDate)?.player
                            ?.GamingCount ??
                        0
                })
            .AsQueryable()
            .CreatePagedResponse(request);
    }
}