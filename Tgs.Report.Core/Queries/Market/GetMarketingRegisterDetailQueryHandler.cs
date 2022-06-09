using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Marketing;

namespace Tgs.Report.Core.Queries.Market;

public class GetMarketingRegisterDetailQueryHandler : IRequestHandler<GetMarketingRegisterDetailRequest,
    PagedResponse<MarketingRegisterDetailModel>>
{
    private readonly TgsContext _context;

    public GetMarketingRegisterDetailQueryHandler(TgsContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PagedResponse<MarketingRegisterDetailModel>> Handle(
        GetMarketingRegisterDetailRequest request,
        CancellationToken cancellationToken)
    {
        var offset = new TimeSpan(8, 0, 0);
        var requestDate = request.Date.ToOffset(offset);

        var reportDate = new DateTimeOffset(requestDate.Year,
            requestDate.Month,
            requestDate.Day,
            0,
            0,
            0,
            requestDate.Offset);

        var agent = await _context.Agents
            .FirstOrDefaultAsync(x => x.BrandId == request.BrandId && x.Username == request.AgentUsername,
                cancellationToken).ConfigureAwait(false);

        if (agent == null)
        {
            return new()
            {
                Valid = true,
                Items = Array.Empty<MarketingRegisterDetailModel>()
            };
        }

        return await _context.PlayerAggregates
            .Where(x => x.BrandId == request.BrandId && x.ReportDate == reportDate.ToUniversalTime() && x.AgentId == agent.Id)
            .Where(x => x.RegisteredCount == 1)
            .GroupBy(
                x => x.PlayerCode,
                (playerCode, group) => new { playerCode })
            .Join(_context.Players, x => x.playerCode, y => y.PlayerCode, (x, y) => new
            {
                y.Username,
                x.playerCode,
                y.RegisteredOn
            }).GroupJoin(_context.PlayerIpRecords, player => player.playerCode, ipr => ipr.PlayerCode,
                (player, ipr) => new
                {
                    player.Username,
                    player.playerCode,
                    player.RegisteredOn,
                    ipr = ipr.Where(x => x.IpType == 2)
                })
            .SelectMany(x => x.ipr.DefaultIfEmpty(), (x, y) => new
            {
                x.Username,
                x.playerCode,
                x.RegisteredOn,
                y.RecordOn
            }).GroupBy(x => new
            {
                x.Username,
                x.playerCode,
                x.RegisteredOn
            }, (player, group) => new
            {
                player.Username,
                player.playerCode,
                player.RegisteredOn,
                LastLoginOn = group.FirstOrDefault().RecordOn
            })
            .GroupJoin(_context.PlayerLoginRecords, player => player.playerCode, login => login.PlayerCode,
                (player, login) => new
                {
                    player.Username,
                    player.playerCode,
                    player.RegisteredOn,
                    player.LastLoginOn,
                    login
                }).SelectMany(x => x.login.DefaultIfEmpty(), (x, y) => new
                {
                    x.Username,
                    x.playerCode,
                    x.RegisteredOn,
                    x.LastLoginOn,
                    y
                }).GroupBy(x => new
                {
                    x.Username,
                    x.playerCode,
                    x.RegisteredOn,
                    x.LastLoginOn
                }, (player, group) => new MarketingRegisterDetailModel
                {
                    Username = player.Username,
                    RegisteredOn = player.RegisteredOn.Value,
                    LastLoginOn = player.LastLoginOn,
                    LoginCount = group.Count(c => c.y.Status == 1)
                }).CreatePagedResponseAsync(request);
    }
}