using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;

namespace Tgs.Report.Core
{
    public interface ITgsService<T>
    {
        ITgsRepository Repository { get; }

        IQueryable<T> GetQueries(
            Func<IQueryable<T>, IQueryable<T>>? preQuery = null,
            bool tracking = false);

        ValueTask CreateEntitiesAsync(T[] entities);
        ValueTask UpdateEntitiesAsync(T[] entities);
        ValueTask DeleteEntitiesAsync(T[] entities);
    }

    public class TgsService<T> : ITgsService<T> where T : class
    {
        private ITgsRepository Repository { get; }

        public TgsService(ITgsRepository repository)
        {
            Repository = repository;
        }

        ITgsRepository ITgsService<T>.Repository => Repository;

        public async ValueTask CreateEntitiesAsync(T[] entities)
        {
            await Repository.Set<T>().AddRangeAsync(entities);
            await Repository.SaveChangesAsync();
        }

        public IQueryable<T> GetQueries(Func<IQueryable<T>, IQueryable<T>> preQuery, bool tracking)
        {
            preQuery ??= c => c;
            return tracking ? preQuery(Repository.Set<T>()) : preQuery(Repository.Set<T>()).AsNoTracking();
        }

        public async ValueTask UpdateEntitiesAsync(T[] entities)
        {
            Repository.Set<T>().UpdateRange(entities);
            await Repository.SaveChangesAsync();
        }

        public async ValueTask DeleteEntitiesAsync(T[] entities)
        {
            Repository.Set<T>().RemoveRange(entities);
            await Repository.SaveChangesAsync();
        }
    }
}