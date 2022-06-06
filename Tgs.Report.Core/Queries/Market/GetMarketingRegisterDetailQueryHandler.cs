using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Market;
using Tgs.Report.ViewModel.Marketing;

namespace Tgs.Report.Core.Queries.Market;

public class GetMarketingRegisterDetailQueryHandler : IRequestHandler<GetMarketingRegisterDetailRequest,
    PagedResponse<MarketingRegisterDetailModel>>
{
    private readonly ITgsRepository _repository;

    public GetMarketingRegisterDetailQueryHandler(ITgsRepository repository) => _repository = repository;

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

        var agent = await _repository.Agents
            .FirstOrDefaultAsync(x => x.Username == request.AgentUsername, cancellationToken).ConfigureAwait(false);

        if (agent == null)
        {
            return new()
            {
                Valid = false,
                Message = Resources.Agent.NotFound
            };
        }

        return await _repository.PlayerAggregates
            .Where(x => x.BrandId == request.BrandId && x.ReportDate == reportDate && x.AgentId == agent.Id)
            .Where(x => x.RegisteredCount == 1)
            .GroupBy(x => x.PlayerCode, (playerCode, group) => new { playerCode })
            .Join(_repository.Players, x => x.playerCode, y => y.PlayerCode, (x, y) => new
            {
                y.Username,
                x.playerCode,
                y.RegisteredOn
            }).GroupJoin(_repository.PlayerIpRecords, player => player.playerCode, ipr => ipr.PlayerCode,
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
            .GroupJoin(_repository.PlayerLoginRecords, player => player.playerCode, login => login.PlayerCode,
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