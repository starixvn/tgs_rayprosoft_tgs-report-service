using MediatR;
using Microsoft.EntityFrameworkCore;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Infrastructures.Extensions.Models;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.PlayerDeposit;

namespace Tgs.Report.Core.Queries.Market;

public class GetPlayerDepositQueryHandler(TgsContext context)
    : IRequestHandler<GetPlayerDepositsRequest, PagedResponse<PlayerDepositModel>>
{
    private readonly TgsContext _context = context ?? throw new ArgumentNullException(nameof(context));

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

        var agent = await _context.Agents
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

        var deposits = await _context.DepositForms
            .Where(x => x.BrandId == request.BrandId && x.Status == 8000)
            .Join(_context.Players, form => form.PlayerCode, player => player.PlayerCode,
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
            .Where(x => x.FirstDepositOn >= from.ToUniversalTime() && x.FirstDepositOn < to.ToUniversalTime())
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);

        var depositFormatDays = deposits.Select(x =>
        {
            var firstDepositOn8Timezone = x.FirstDepositOn.ToOffset(offset);
            var firstDepositOnDate = new DateTimeOffset(firstDepositOn8Timezone.Year, firstDepositOn8Timezone.Month,
                firstDepositOn8Timezone.Day, 0, 0, 0, offset);
            return new
            {
                x.PlayerCode,
                x.FirstDepositOn,
                x.LastDepositOn,
                FirstDepositOnDate = firstDepositOnDate
            };
        }).ToArray();

        return dates.GroupJoin(depositFormatDays,
                date => date,
                deposit => deposit.FirstDepositOnDate,
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
            .AsQueryable()
            .CreatePagedResponse(request);
    }
}