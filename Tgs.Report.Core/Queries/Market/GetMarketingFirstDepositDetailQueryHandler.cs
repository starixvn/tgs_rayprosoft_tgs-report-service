using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Market;
using Tgs.Report.ViewModel.Marketing;
using Tgs.Report.ViewModel.PlayerDeposit;

namespace Tgs.Report.Core.Queries.Market;

public class GetMarketingFirstDepositDetailQueryHandler : IRequestHandler<GetMarketingFirstDepositDetailRequest,
    PagedResponse<MarketingFirstDepositDetailModel>>
{
    private readonly IMediator _mediator;
    private readonly ITgsRepository _repository;

    public GetMarketingFirstDepositDetailQueryHandler(ITgsRepository repository, IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
    }

    public async Task<PagedResponse<MarketingFirstDepositDetailModel>> Handle(
        GetMarketingFirstDepositDetailRequest request,
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
                Valid = true,
                Items = Array.Empty<MarketingFirstDepositDetailModel>()
            };
        }

        var details = await _mediator.Send(new GetPlayerFirstDepositDetailsRequest
        {
            BrandId = request.BrandId,
            AgentUsername = request.AgentUsername,
            Date = request.Date,
            ShowAll = true
        }, cancellationToken).ConfigureAwait(false);

        var playerUsernames = await _repository.PlayerAggregates
            .Where(x => x.BrandId == request.BrandId && x.ReportDate == reportDate && x.AgentId == agent.Id)
            .Where(x => x.FirstFundInCount == 1)
            .Join(_repository.Players, aggregate => aggregate.PlayerCode, player => player.PlayerCode,
                (aggregate, player) => new
                {
                    player.Username,
                    aggregate.PlayerCode
                })
            .GroupBy(x => x.Username, (username, group) => new { Username = username })
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);

        return await playerUsernames
            .Join(details.Items!, p => p.Username, y => y.Username,
                (username, detail) => new MarketingFirstDepositDetailModel
                {
                    Username = detail.Username,
                    FirstDepositOn = detail.FirstDepositOn,
                    FirstDepositAmount = detail.FirstDepositAmount,
                    TotalDepositAmount = detail.TotalDepositAmount,
                    TotalWithdrawAmount = detail.TotalWithdrawAmount,
                    DepositSubtractWithdraw = detail.DepositSubtractWithdraw,
                    IpAddress = detail.IpAddress
                }).CreatePagedResponseAsync(playerUsernames.Length);
    }
}