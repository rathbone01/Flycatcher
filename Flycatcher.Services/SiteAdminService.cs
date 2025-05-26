using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
namespace Flycatcher.Services
{
    public class SiteAdminService
    {
        private readonly IQueryableRepository<User> userQueryableRepository;
        private readonly IQueryableRepository<Server> serverQueryableRepository;
        public SiteAdminService(IQueryableRepository<User> userQueryableRepository, IQueryableRepository<Server> serverQueryableRepository)
        {
            this.userQueryableRepository = userQueryableRepository;
            this.serverQueryableRepository = serverQueryableRepository;
        }

        public List<User> GetAllUserPaged(int pageSize, int pageNumber)
        {
            return userQueryableRepository
                .GetQueryable()
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToList();
        }

        public int GetAllUserCount()
        {
            return userQueryableRepository
                .GetQueryable()
                .Count();
        }

        public List<Server> GetAllServersPaged(int pageSize, int pageNumber)
        {
            return serverQueryableRepository
                .GetQueryable()
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToList();
        }

        public int GetAllServersCount()
        {
            return serverQueryableRepository
                .GetQueryable()
                .Count();
        }
    }
}
