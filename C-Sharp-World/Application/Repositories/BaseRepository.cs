using IdentityVerificationService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IdentityVerificationService.Application.Repositories
{
    public class BaseRepository
    {
        protected readonly CaaryDbContext _caaryDbContext;

        public BaseRepository(CaaryDbContext caaryDbContext)
        {
            _caaryDbContext = caaryDbContext ?? throw new ArgumentNullException($"{nameof(caaryDbContext)}");
        }

        public async Task<(T, bool)> AddEntityAsync<T>(T entity, CancellationToken cancellationToken)
            where T : class
        {
            _caaryDbContext.Set<T>().AddRange(entity);

            var isAdded = await _caaryDbContext.SaveChangesAsync(cancellationToken) > 0;

            return (entity, isAdded);
        }

        public async Task<(IEnumerable<T>, bool)> AddEntityAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default)
            where T : class
        {
            _caaryDbContext.Set<T>().AddRange(entities);

            var isAdded = await _caaryDbContext.SaveChangesAsync(cancellationToken) > 0;

            return (entities, isAdded);
        }

        public async Task<(T, bool)> UpdateEntityAsync<T>(T entity, CancellationToken cancellationToken = default)
            where T : class
        {
            _caaryDbContext.Entry(entity).State = EntityState.Modified;

            var isUpdated = await _caaryDbContext.SaveChangesAsync(cancellationToken) > 0;

            return (entity, isUpdated);
        }

        public async Task<(IEnumerable<T>, bool)> UpdateEntityAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default)
            where T : class
        {
            foreach (T entity in entities)
            {
                _caaryDbContext.Entry(entity).State = EntityState.Modified;
            }

            var isUpdated = await _caaryDbContext.SaveChangesAsync(cancellationToken) > 0;

            return (entities, isUpdated);
        }

        public async Task<(T, bool)> DeleteEntityAsync<T>(T entity, CancellationToken cancellationToken = default)
            where T : class
        {
            _caaryDbContext.Set<T>().Remove(entity);

            var isDeleted = await _caaryDbContext.SaveChangesAsync(cancellationToken) > 0;

            return (entity, isDeleted);
        }

        public async Task<(IEnumerable<T>, bool)> DeleteEntityAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default)
            where T : class
        {
            foreach (T entity in entities)
            {
                _caaryDbContext.Entry(entity).State = EntityState.Deleted;
            }

            var isDeleted = await _caaryDbContext.SaveChangesAsync(cancellationToken) > 0;

            return (entities, isDeleted);
        }
    }
}
