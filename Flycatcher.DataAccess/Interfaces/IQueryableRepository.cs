using System.Linq.Expressions;

namespace Flycatcher.DataAccess.Interfaces
{
    public interface IQueryableRepository<T> where T : class
    {
        public Task<TResult> ExecuteAsync<TResult>(Func<IQueryable<T>, Task<TResult>> query, CancellationToken token = default);
        public Task<T> Create(T entity);
        public Task Delete(T entity);
        public Task<T> Update(T entity);
        public Task<int> ExecuteDelete(Expression<Func<T, bool>> predicate);
    }
}
