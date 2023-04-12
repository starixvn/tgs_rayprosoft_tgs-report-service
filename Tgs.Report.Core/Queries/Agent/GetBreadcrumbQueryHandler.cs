using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent;

internal class GetBreadcrumbQueryHandler : IRequestHandler<GetBreadcrumbRequest,
    IEnumerable<BreadcrumbModel>>
{
    private readonly TgsContext _context;

    public GetBreadcrumbQueryHandler(TgsContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }


    public async Task<IEnumerable<BreadcrumbModel>> Handle(
        GetBreadcrumbRequest request,
        CancellationToken cancellationToken)
    {
        var playerCode = request.PlayerCode;

        var agentId = request.AgentId;

        var round = request.RoundId != null
            ? await _context.Rounds.FirstOrDefaultAsync(x => x.Id == request.RoundId, cancellationToken)
            : null;

        if (round != null)
        {
            playerCode = round?.PlayerCode;
        }

        var player = !string.IsNullOrEmpty(playerCode)
            ? await _context.Players.FirstOrDefaultAsync(x => x.PlayerCode == playerCode, cancellationToken)
            : null;

        if (player != null)
        {
            agentId = player?.AgentId;
        }

        if (agentId == null)
        {
            return Array.Empty<BreadcrumbModel>();
        }

        var agents = await _context.Agents.FromSql($@"select
  a.*
from
  dbo.agents f
  inner join
    dbo.agents a
    on
      (
        a.path @> f.path
      )
where
  f.id={agentId}").ToArrayAsync(cancellationToken);

        var pathList = agents.Select(x => new BreadcrumbModel
        {
            Id = x.Id.ToString(),
            Name = x.Username,
            Type = "agent"
        });

        if (player != null)
        {
            pathList = pathList.Concat(new[]
            {
                new BreadcrumbModel
                {
                    Id = player.PlayerCode,
                    Name = player.Username,
                    Type = "player"
                }
            });
        }

        if (round != null)
        {
            pathList = pathList.Concat(new[]
            {
                new BreadcrumbModel
                {
                    Id = round.Id.ToString(),
                    Name = round.Id.ToString(),
                    Type = "round"
                }
            });
        }

        return pathList;
    }
}