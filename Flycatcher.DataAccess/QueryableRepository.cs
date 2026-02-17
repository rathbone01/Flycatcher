using Flycatcher.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Flycatcher.DataAccess
{
    public class QueryableRepository<T> : IQueryableRepository<T> where T : class
    {
        private readonly ContextFactory contextFactory;
        public QueryableRepository(ContextFactory contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<IQueryable<T>, Task<TResult>> query, CancellationToken token = default)
        {
            await using var dbContext = contextFactory.CreateDbContext();
            return await query(dbContext.Set<T>().AsQueryable());
        }

        public async Task<T> Create(T entity)
        {
            await using var dbContext = contextFactory.CreateDbContext();
            dbContext.Add(entity);
            await dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task Delete(T entity)
        {
            await using var dbContext = contextFactory.CreateDbContext();
            dbContext.Remove(entity);
            await dbContext.SaveChangesAsync();
        }

        public async Task<T> Update(T entity)
        {
            await using var dbContext = contextFactory.CreateDbContext();
            dbContext.Update(entity);
            await dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<int> ExecuteDelete(Expression<Func<T, bool>> predicate)
        {
            await using var dbContext = contextFactory.CreateDbContext();
            return await dbContext.Set<T>().Where(predicate).ExecuteDeleteAsync();
        }
    }
}
