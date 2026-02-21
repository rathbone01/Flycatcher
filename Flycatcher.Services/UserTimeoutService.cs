using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Flycatcher.Services
{
    public class UserTimeoutService
    {
        private readonly IQueryableRepository<UserTimeout> userTimeoutRepository;
        private readonly IQueryableRepository<Server> serverRepository;
        private readonly IQueryableRepository<SiteAdmin> siteAdminRepository;

        public UserTimeoutService(
            IQueryableRepository<UserTimeout> userTimeoutRepository,
            IQueryableRepository<Server> serverRepository,
            IQueryableRepository<SiteAdmin> siteAdminRepository)
        {
            this.userTimeoutRepository = userTimeoutRepository;
            this.serverRepository = serverRepository;
            this.siteAdminRepository = siteAdminRepository;
        }

        public async Task<bool> IsUserTimedOutAsync(int userId, int serverId)
        {
            var now = DateTime.UtcNow;

            return await userTimeoutRepository
                .ExecuteAsync(q => q.AnyAsync(ut =>
                    ut.UserId == userId &&
                    ut.ServerId == serverId &&
                    ut.ExpiresAtUtc > now));
        }

        public async Task<UserTimeout?> GetActiveTimeoutAsync(int userId, int serverId)
        {
            var now = DateTime.UtcNow;

            return await userTimeoutRepository
                .ExecuteAsync(q => q
                    .FirstOrDefaultAsync(ut =>
                        ut.UserId == userId &&
                        ut.ServerId == serverId &&
                        ut.ExpiresAtUtc > now));
        }

        public async Task<string?> TimeoutUserAsync(int userId, int serverId, int timeoutByUserId, string reason, int durationMinutes)
        {
            if (userId == timeoutByUserId)
                return "You cannot timeout yourself.";

            var targetIsSiteAdmin = await siteAdminRepository
                .ExecuteAsync(q => q.AnyAsync(sa => sa.UserId == userId));
            if (targetIsSiteAdmin)
                return "Site administrators cannot be timed out.";

            var server = await serverRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(s => s.Id == serverId));
            if (server is not null && server.OwnerUserId == userId)
                return "The server owner cannot be timed out.";

            if (durationMinutes <= 0)
                return "Duration must be greater than 0 minutes.";

            var now = DateTime.UtcNow;

            var existingTimeout = await userTimeoutRepository
                .ExecuteAsync(q => q
                    .FirstOrDefaultAsync(ut =>
                        ut.UserId == userId &&
                        ut.ServerId == serverId &&
                        ut.ExpiresAtUtc > now));

            if (existingTimeout is not null)
            {
                existingTimeout.TimeoutByUserId = timeoutByUserId;
                existingTimeout.Reason = reason;
                existingTimeout.TimeoutAtUtc = now;
                existingTimeout.ExpiresAtUtc = now.AddMinutes(durationMinutes);

                await userTimeoutRepository.Update(existingTimeout);
            }
            else
            {
                var newTimeout = new UserTimeout
                {
                    UserId = userId,
                    ServerId = serverId,
                    TimeoutByUserId = timeoutByUserId,
                    Reason = reason,
                    TimeoutAtUtc = now,
                    ExpiresAtUtc = now.AddMinutes(durationMinutes)
                };

                await userTimeoutRepository.Create(newTimeout);
            }

            return null;
        }

        public async Task<string?> RemoveTimeoutAsync(int userId, int serverId)
        {
            var now = DateTime.UtcNow;

            var activeTimeout = await userTimeoutRepository
                .ExecuteAsync(q => q
                    .FirstOrDefaultAsync(ut =>
                        ut.UserId == userId &&
                        ut.ServerId == serverId &&
                        ut.ExpiresAtUtc > now));

            if (activeTimeout is null)
                return "No active timeout found.";

            await userTimeoutRepository.Delete(activeTimeout);

            return null;
        }

        public async Task<List<UserTimeout>> GetServerActiveTimeoutsAsync(int serverId)
        {
            var now = DateTime.UtcNow;

            return await userTimeoutRepository
                .ExecuteAsync(q => q
                    .Where(ut =>
                        ut.ServerId == serverId &&
                        ut.ExpiresAtUtc > now)
                    .Include(ut => ut.User)
                    .Include(ut => ut.TimeoutByUser)
                    .OrderByDescending(ut => ut.TimeoutAtUtc)
                    .ToListAsync());
        }
    }
}
