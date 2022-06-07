using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.PlayerDeposit;

namespace Tgs.Report.Core.Queries.Market;

public class GetPlayerDepositQueryHandler : IRequestHandler<GetPlayerDepositsRequest, PagedResponse<PlayerDepositModel>>
{
    private readonly ITgsRepository _repository;

    public GetPlayerDepositQueryHandler(ITgsRepository repository) => _repository = repository;

    public async Task<PagedResponse<PlayerDepositModel>> Handle(
        GetPlayerDepositsRequest request,
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
                Items = Array.Empty<PlayerDepositModel>()
            };
        }

        var dates = Enumerable.Range(0, to.Subtract(from).Days)
            .Select(x => from.AddDays(x))
            .ToArray();

        var deposits = _repository.DepositForms
            .Where(x => x.BrandId == request.BrandId && x.Status == 8000)
            .Join(_repository.Players, form => form.PlayerCode, player => player.PlayerCode,
                (form, player) => new
                {
                    form.PlayerCode,
                    form.CreatedOn,
                    player.AgentId
                })
            .Where(x => x.AgentId == agent.Id)
            .GroupBy(x => x.PlayerCode, (playerCode, group) => new
            {
                PlayerCode = playerCode,
                FirstDepositOn = group.Min(x => x.CreatedOn),
                LastDepositOn = group.Max(x => x.CreatedOn)
            })
            .Where(x => x.FirstDepositOn >= from.ToUniversalTime() && x.FirstDepositOn < to.ToUniversalTime());

        return await dates.GroupJoin(deposits,
                date => date,
                deposit => deposit.FirstDepositOn.Date,
                (date, groupDeposits) => new
                {
                    date,
                    groupDeposits
                })
            .SelectMany(x => x.groupDeposits.DefaultIfEmpty(),
                (x, deposit) => new
                {
                    ReportDate = x.date,
                    deposit
                })
            .GroupBy(x => new { x.ReportDate },
                (deposit, groupDeposits) => new PlayerDepositModel
                {
                    ReportDate = deposit.ReportDate,
                    FirstDepositCount = groupDeposits.Count(c => c.deposit != null),
                    NoSecondDepositCount = groupDeposits.Count(x
                        => x.deposit != null && x.deposit.FirstDepositOn == x.deposit.LastDepositOn)
                })
            .CreatePagedResponseAsync(dates.Length);
    }
}