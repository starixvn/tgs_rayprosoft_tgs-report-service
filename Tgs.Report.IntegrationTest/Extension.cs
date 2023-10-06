using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Tgs.Report.IntegrationTest;

public static class Extension
{
    public static DbSet<T> Initialize<T>(this DbSet<T> dbSet, IQueryable<T> data) where T : class
    {
        _ = ((IQueryable<T>)dbSet).Provider.Returns(data.Provider);
        _ = ((IQueryable<T>)dbSet).Expression.Returns(data.Expression);
        _ = ((IQueryable<T>)dbSet).ElementType.Returns(data.ElementType);
        _ = ((IQueryable<T>)dbSet).GetEnumerator().Returns(data.GetEnumerator());

        return dbSet;
    }
}