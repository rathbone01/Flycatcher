using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flycatcher.DataAccess.Interfaces
{
    public interface IQueryableRepository
    {
        public IQueryable<T> GetQueryable<T>() where T : class;
        public void Create<T>(T entity) where T : class;
        public void Delete<T>(T entity) where T : class;
        public void Update<T>(T entity) where T : class;
        public Task SaveChangesAsync();
    }
}
