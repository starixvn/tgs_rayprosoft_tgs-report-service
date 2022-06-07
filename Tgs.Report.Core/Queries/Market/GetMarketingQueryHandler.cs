using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Market;
using Tgs.Report.ViewModel.Marketing;

namespace Tgs.Report.Core.Queries.Market;

public class GetMarketingQueryHandler : IRequestHandler<GetMarketingListRequest, PagedResponse<MarketingModel>>
{
    private readonly ITgsRepository _repository;

    public GetMarketingQueryHandler(ITgsRepository repository) => _repository = repository;

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

        var agent = await _repository.Agents
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
        
        var playerAggregates = await _repository.PlayerAggregates
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.AgentId == agent.Id)
            .GroupBy(x => new { x.ReportDate, x.PlayerCode }, (player, group) => new MarketingModel
            {
                ReportDate = player.ReportDate,
                RegisterCount = group.Sum(x => x.RegisteredCount),
                FirstDepositCount = group.Sum(x => x.FirstFundInCount),
                GamingCount = group.Count()
            }).ToArrayAsync(cancellationToken).ConfigureAwait(false);

        return await dates.GroupJoin(playerAggregates,
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
                    RegisterCount = groupDeposits.FirstOrDefault(x => date.ReportDate == x.ReportDate)?.player?.RegisterCount ?? 0,
                    FirstDepositCount = groupDeposits.FirstOrDefault(x => date.ReportDate == x.ReportDate)?.player?.FirstDepositCount ?? 0,
                    GamingCount = groupDeposits.FirstOrDefault(x => date.ReportDate == x.ReportDate)?.player?.GamingCount ?? 0
                })
            .CreatePagedResponseAsync(dates.Length);
    }
}