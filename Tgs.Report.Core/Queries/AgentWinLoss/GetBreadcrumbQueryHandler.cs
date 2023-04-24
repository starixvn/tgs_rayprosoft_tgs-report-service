using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Agent;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core.Queries.AgentWinLoss;

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
            playerCode = round.PlayerCode;
        }

        var player = !string.IsNullOrEmpty(playerCode)
            ? await _context.Players.FirstOrDefaultAsync(x => x.PlayerCode == playerCode, cancellationToken)
            : null;

        if (player != null)
        {
            agentId = player.AgentId;
        }

        if (agentId == null)
            return Array.Empty<BreadcrumbModel>();

        var agent = await _context.Agents.Select(x => new
        {
            x.Id,
            x.Path
        }).FirstOrDefaultAsync(x => x.Id == agentId, cancellationToken);

        if (agent == null)
            return Array.Empty<BreadcrumbModel>();

        var agents = await _context.Agents
            .Where(x => x.Path.IsAncestorOf(agent.Path))
            .Select(x => new
            {
                x.Id,
                x.Username,
                x.Path.NLevel
            })
            .ToArrayAsync(cancellationToken);

        var pathList = agents.OrderBy(x => x.NLevel).Select(x => new BreadcrumbModel
        {
            Id = x.Id.ToString(),
            Name = x.Username,
            Type = "agent"
        }).ToList();

        if (request.RoundId != null && player != null)
        {
            pathList.Add(new BreadcrumbModel
            {
                Id = player.PlayerCode,
                Name = player.Username,
                Type = "player"
            });
        }
        else
        {
            pathList.RemoveAt(pathList.Count - 1);
        }

        return pathList;
    }
}