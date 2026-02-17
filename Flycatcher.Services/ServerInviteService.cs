using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Flycatcher.Models.Results;
using Microsoft.EntityFrameworkCore;
using Flycatcher.Services.Enumerations;

namespace Flycatcher.Services
{
    public class ServerInviteService
    {
        private readonly IQueryableRepository<ServerInvite> serverInviteQueryableRepository;
        private readonly IQueryableRepository<Server> serverQueryableRepository;
        private readonly IQueryableRepository<User> userQueryableRepository;
        private readonly IQueryableRepository<UserServer> userServerQueryableRepository;
        private readonly CallbackService callbackService;

        public ServerInviteService(IQueryableRepository<ServerInvite> serverInviteQueryableRepository, IQueryableRepository<Server> serverQueryableRepository, IQueryableRepository<User> userQueryableRepository, IQueryableRepository<UserServer> userServerQueryableRepository, CallbackService callbackService)
        {
            this.serverInviteQueryableRepository = serverInviteQueryableRepository;
            this.callbackService = callbackService;
            this.serverQueryableRepository = serverQueryableRepository;
            this.userQueryableRepository = userQueryableRepository;
            this.userServerQueryableRepository = userServerQueryableRepository;
        }

        public async Task<int> GetServerInvitesCount(int userId)
        {
            return await serverInviteQueryableRepository
                .ExecuteAsync(q => q
                    .Where(si => si.RecieverUserId == userId)
                    .Include(si => si.Server)
                    .CountAsync());
        }

        public async Task<List<ServerInvite>> GetServerInvites(int userId)
        {
            return await serverInviteQueryableRepository
                .ExecuteAsync(q => q
                    .Where(si => si.RecieverUserId == userId)
                    .Include(si => si.Server)
                    .ToListAsync());
        }

        public async Task<Result> CreateInvite(int serverId, int recieverUserId, int senderUserId)
        {
            if (await serverInviteQueryableRepository.ExecuteAsync(q => q.AnyAsync(si => si.ServerId == serverId && si.RecieverUserId == recieverUserId)))
                return new Result(false, "Invite already exists.");

            if (!await serverQueryableRepository.ExecuteAsync(q => q.AnyAsync(s => s.Id == serverId)))
                return new Result(false, "Server not found.");

            if (!await userQueryableRepository.ExecuteAsync(q => q.AnyAsync(u => u.Id == recieverUserId)))
                return new Result(false, "Reciever not found.");

            if (!await userQueryableRepository.ExecuteAsync(q => q.AnyAsync(u => u.Id == senderUserId)))
                return new Result(false, "Sender not found.");

            var invite = new ServerInvite
            {
                ServerId = serverId,
                RecieverUserId = recieverUserId,
                SenderUserId = senderUserId
            };

            await serverInviteQueryableRepository.Create(invite);
            await callbackService.NotifyAsync(CallbackType.ServerInvite, recieverUserId);

            return new Result(true);
        }

        public async Task<Result> DeleteInvite(int serverInviteId)
        {
            var invite = await serverInviteQueryableRepository.ExecuteAsync(q => q.FirstOrDefaultAsync(si => si.Id == serverInviteId));
            if (invite is null)
                return new Result(false, "Invite not found.");

            var recieverUserId = invite.RecieverUserId;

            await serverInviteQueryableRepository.Delete(invite);
            await callbackService.NotifyAsync(CallbackType.ServerInvite, recieverUserId);

            return new Result(true);
        }

        public async Task<Result> AcceptInvite(int serverInviteId)
        {
            var invite = await serverInviteQueryableRepository.ExecuteAsync(q => q.FirstOrDefaultAsync(si => si.Id == serverInviteId));
            if (invite is null)
                return new Result(false, "Invite not found.");

            var server = await serverQueryableRepository.ExecuteAsync(q => q.FirstOrDefaultAsync(s => s.Id == invite.ServerId));
            if (server is null)
                return new Result(false, "Server not found.");

            var reciever = await userQueryableRepository.ExecuteAsync(q => q.FirstOrDefaultAsync(u => u.Id == invite.RecieverUserId));
            if (reciever is null)
                return new Result(false, "Reciever not found.");

            var userServer = new UserServer
            {
                UserId = invite.RecieverUserId,
                ServerId = invite.ServerId
            };

            await userServerQueryableRepository.Create(userServer);
            await serverInviteQueryableRepository.Delete(invite);

            await callbackService.NotifyAsync(CallbackType.UserServerListUpdated, userServer.UserId);
            await callbackService.NotifyAsync(CallbackType.ServerUserUpdated, userServer.ServerId);

            return new Result(true);
        }
    }
}
