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
        var spName = @"
                    with licenseeIds
                    as (select l.id, l.reference_id
                        from dbo.licensees as l
	                    where 
	                          (coalesce(TRIM(@referenceId), '') != '' and l.reference_id = @referenceId)
	                          or 
		                      (coalesce(TRIM(@referenceId), '') = '' and l.reference_id != '')
	                    )
                    select 
                           gp.code as GameProviderCode,
                           date(r.external_closed_on) as WinLossDate,
	                       sum(r.win_loss) as PlayerWinLoss,
	                       ga.game_provider_currency_code as GameProviderCurrencyCode,
	                       0 as TypeId,
	                       l.reference_id as ReferenceId
                    from bet.rounds as r
	                    join dbo.brands as b on b.id = r.brand_id
	                    join licenseeIds as l on l.id = b.licensee_id
	                    join dbo.wallets as w on w.id = r.wallet_id
	                    join dbo.game_provider_currencies as ga 
	                         on ga.game_provider_id = r.game_provider_id 
	                         and ga.currency_code = w.currency_code
	                    join dbo.game_providers as gp on gp.id = ga.game_provider_id
	
                    where r.status = 2
	                      and r.external_closed_on >= @from
	                      and r.external_closed_on < @to
                    group by 
                             date(r.external_closed_on),
		                     ga.game_provider_currency_code,
		                     gp.code,
                             l.reference_id";
        var conn = context.Database.GetDbConnection();
        var transaction = context.Database.CurrentTransaction?.GetDbTransaction();
        var result = await conn.QueryAsync<PlayerWinLossModel>(spName,
            new Dictionary<string, object>
            {
                { "referenceId", string.IsNullOrEmpty(request.ReferenceId) ? string.Empty : request.ReferenceId },
                { "from", request.From.ToUniversalTime() },
                { "to", request.To.ToUniversalTime() }
            }, transaction);

        foreach (var item in result)
        {
            if(!string.IsNullOrEmpty(item.WinLossDate))
            {
                item.WinLossDate = DateTime.Parse(item.WinLossDate).ToString("yyyy-MM-dd HH:mm:ss tt +00:00");
            }
        };

        return result;
    }
}
