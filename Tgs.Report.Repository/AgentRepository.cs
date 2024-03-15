using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;

namespace Tgs.Report.Repository;

internal class AgentRepository(TgsContext context) : IAgentRepository
{
    private readonly TgsContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public Task<AgentWithNLevelInfo?> GetAgentWithNLevelAsync(
        Guid brandId,
        Guid? agentId,
        string? userName,
        CancellationToken cancellationToken)
    {
        var query = _context.Agents
            .Where(a => a.BrandId == brandId);

        if (agentId != null)
        {
            query = query.Where(a => a.Id == agentId);
        }

        if (!string.IsNullOrEmpty(userName))
        {
            query = query.Where(x => x.Username == userName);
        }

        return query.Select(a => new AgentWithNLevelInfo(
                a.Id,
                a.Path.ToString(),
                a.Path.NLevel))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public IAsyncEnumerable<AgentInfo> GetFirstLayerAgentsAsync(Guid brandId, Guid parentAgentId)
        => _context.Agents
            .Where(a => a.BrandId == brandId && a.ParentAgentId == parentAgentId)
            .Select(a => new AgentInfo(a.Id, a.Username))
            .AsAsyncEnumerable();

    public IAsyncEnumerable<ChildAgentInfo> GetChildAgentsAsync(Guid brandId, AgentWithNLevelInfo parentAgentInfo)
    {
        var nlevel = parentAgentInfo.NLevel;

        return _context.Agents
            .Where(a => a.BrandId == brandId)
            .Where(a => a.Path.IsDescendantOf(new(parentAgentInfo.Path)))
            .Where(a => a.Id != parentAgentInfo.Id)
            .Select(a => new ChildAgentInfo(
                a.Id,
                Guid.Parse(a.Path.Subtree(nlevel, nlevel + 1))))
            .AsAsyncEnumerable();
    }
}