using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.LicenseeWinLoss;
using Dapper;

namespace Tgs.Report.Core.Queries.LicenseeWinLoss;

public class GetPlayerWinLossByRefIdQueryHandler(TgsContext context) : IRequestHandler<GetPlayerWinLossByRefIdRequest, IEnumerable<PlayerWinLossModel>>
{
    public async Task<IEnumerable<PlayerWinLossModel>> Handle(GetPlayerWinLossByRefIdRequest request, CancellationToken cancellationToken)
    {
        var timeOffset = request.From.Offset;
        var timeZone = $"{(timeOffset < TimeSpan.Zero ? "-" : "+")}{timeOffset:hh\\:mm}";
        var spName = @"
                    with licenseeIds
                    as (select l.id, l.reference_id
                        from dbo.licensees as l
	                    where 
	                          (array_length(@referenceIds, 1) > 0 and l.reference_id = any(@referenceIds))
	                          or 
		                      (array_length(@referenceIds, 1) is null and l.reference_id != '')
	                    )
                    select 
                           gp.code as GameProviderCode,
                           date_trunc('day', external_closed_on AT TIME ZONE 'UTC' AT TIME ZONE @timeZone) as WinLossDate,
	                       sum(r.win_loss) as PlayerWinLoss,
	                       0 as TypeId,
	                       l.reference_id as ReferenceId
                    from bet.rounds as r
	                    join dbo.brands as b on b.id = r.brand_id
	                    join licenseeIds as l on l.id = b.licensee_id
	                    join dbo.game_providers as gp on gp.id = r.game_provider_id
                    where r.status = 2
	                      and r.external_closed_on >= @from
	                      and r.external_closed_on < @to
                    group by 
                             date_trunc('day', external_closed_on AT TIME ZONE 'UTC' AT TIME ZONE @timeZone),
		                     gp.code,
                             l.reference_id";
        var conn = context.Database.GetDbConnection();
        var transaction = context.Database.CurrentTransaction?.GetDbTransaction();
        var result = await conn.QueryAsync<PlayerWinLossModel>(spName,
            new Dictionary<string, object>
            {
                { "referenceIds", string.IsNullOrEmpty(request.ReferenceIds) ?  new List<string>() : request.ReferenceIds.Split(",").ToList() },
                { "from", request.From.ToUniversalTime() },
                { "to", request.To.ToUniversalTime().AddDays(1) },
                { "timeZone", timeZone},
            }, transaction);

        foreach (var item in result)
        {
            if (!string.IsNullOrEmpty(item.WinLossDate))
            {
                item.WinLossDate = DateTime.Parse(item.WinLossDate).ToString($"yyyy-MM-dd HH:mm:ss tt {timeZone}");
            }
        };

        return result;
    }
}
