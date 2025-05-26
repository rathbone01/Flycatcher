using Flycatcher.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Flycatcher.DataAccess
{
    public class QueryableRepository<T> : IQueryableRepository <T> where T : class
    {
        private readonly ContextFactory contextFactory;
        public QueryableRepository(ContextFactory contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public IQueryable<T> GetQueryable()
        {
            var dbContext = contextFactory.CreateDbContext();
            return dbContext.Set<T>();
        }

        public async Task<T> Create(T entity)
        {
            var dbContext = contextFactory.CreateDbContext();
            dbContext.Add(entity);
            await dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task Delete(T entity)
        {
            var dbContext = contextFactory.CreateDbContext();
            dbContext.Remove(entity);
            await dbContext.SaveChangesAsync();
        }

        public async Task<T> Update(T entity)
        {
            var dbContext = contextFactory.CreateDbContext();
            dbContext.Update(entity);
            await dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<int> ExecuteDelete(Expression<Func<T, bool>> predicate)
        {
            var dbContext = contextFactory.CreateDbContext();
            return await dbContext.Set<T>().Where(predicate).ExecuteDeleteAsync();
        }
    }
}
