using Flycatcher.DataAccess.Interfaces;

namespace Flycatcher.DataAccess
{
    public class QueryableRepository(DataContext dbContext) : IQueryableRepository
    {
        private readonly DataContext dbContext = dbContext;

        public IQueryable<T> GetQueryable<T>() where T : class
        {
            return dbContext.Set<T>();
        }

        public void Create<T>(T entity) where T : class
        {
            dbContext.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            dbContext.Remove(entity);
        }

        public void Update<T>(T entity) where T : class
        {
            dbContext.Update(entity);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}
