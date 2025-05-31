using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Microsoft.EntityFrameworkCore;

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

        public async Task<List<User>> GetAllUserPaged(int pageSize, int pageNumber)
        {
            return await userQueryableRepository
                .GetQueryable()
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();
        }

        public int GetAllUserCount()
        {
            return userQueryableRepository
                .GetQueryable()
                .Count();
        }

        public async Task<List<Server>> GetAllServersPaged(int pageSize, int pageNumber)
        {
            return await serverQueryableRepository
                .GetQueryable()
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetAllServersCount()
        {
            return  await serverQueryableRepository
                .GetQueryable()
                .CountAsync();
        }
    }
}
