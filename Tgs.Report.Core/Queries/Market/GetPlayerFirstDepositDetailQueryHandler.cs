using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.PlayerDeposit;

namespace Tgs.Report.Core.Queries.Market;

public class GetPlayerDepositFirstDetailQueryHandler
    : IRequestHandler<GetPlayerFirstDepositDetailsRequest, PagedResponse<PlayerFirstDepositDetailModel>>
{
    private readonly TgsContext _context;
    private readonly IPlayerDepositFactory _playerDepositFactory;

    public GetPlayerDepositFirstDetailQueryHandler(
        IPlayerDepositFactory playerDepositFactory,
        TgsContext context)
    {
        _playerDepositFactory = playerDepositFactory ?? throw new ArgumentNullException(nameof(playerDepositFactory));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PagedResponse<PlayerFirstDepositDetailModel>> Handle(
        GetPlayerFirstDepositDetailsRequest request,
        CancellationToken cancellationToken)
    {
        var offset = new TimeSpan(8, 0, 0);
        var requestFrom = request.Date.ToOffset(offset);
        var requestTo = request.Date.ToOffset(offset).AddDays(1);

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
                Items = Array.Empty<PlayerFirstDepositDetailModel>()
            };
        }

        var details = await _playerDepositFactory.GetPlayerFirstDepositDetailsAsync(request.BrandId, agent.Id,
            from.ToUniversalTime(), to.ToUniversalTime());

        return details.AsQueryable().CreatePagedResponse(request);
    }
}