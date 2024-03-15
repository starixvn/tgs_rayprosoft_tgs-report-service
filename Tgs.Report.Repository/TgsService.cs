using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Entities;

namespace Tgs.Report.Repository;

public interface ITgsService<T>
{
    IQueryable<T> GetQueries(
        Func<IQueryable<T>, IQueryable<T>>? preQuery = null,
        bool tracking = false);
}

public class TgsService<T>(TgsContext context) : ITgsService<T>
    where T : class
{
    private readonly TgsContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public IQueryable<T> GetQueries(Func<IQueryable<T>, IQueryable<T>>? preQuery, bool tracking)
    {
        preQuery ??= c => c;
        return tracking ? preQuery(_context.Set<T>()) : preQuery(_context.Set<T>()).AsNoTracking();
    }
}