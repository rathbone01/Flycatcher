using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
namespace Flycatcher.Services
{
    public class SiteAdminService
    {
        private readonly IQueryableRepository queryableRepository;
        public SiteAdminService(IQueryableRepository queryableRepository)
        {
            this.queryableRepository = queryableRepository;
        }

        public List<User> GetAllUserPaged(int pageSize, int pageNumber)
        {
            return queryableRepository
                .GetQueryable<User>()
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToList();
        }

        public int GetAllUserCount()
        {
            return queryableRepository
                .GetQueryable<User>()
                .Count();
        }

        public List<Server> GetAllServersPaged(int pageSize, int pageNumber)
        {
            return queryableRepository
                .GetQueryable<Server>()
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToList();
        }

        public int GetAllServersCount()
        {
            return queryableRepository
                .GetQueryable<Server>()
                .Count();
        }
    }
}
