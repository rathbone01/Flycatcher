using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Flycatcher.Models.Results;
using Microsoft.EntityFrameworkCore;

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

        public int GetServerInvitesCount(int userId)
        {
            return serverInviteQueryableRepository
                .GetQueryable()
                .Where(si => si.RecieverUserId == userId)
                .Include(si => si.Server)
                .Count();
        }

        public List<ServerInvite> GetServerInvites(int userId)
        {
            return serverInviteQueryableRepository
                .GetQueryable()
                .Where(si => si.RecieverUserId == userId)
                .Include(si => si.Server)
                .ToList();
        }

        public async Task<Result> CreateInvite(int serverId, int recieverUserId, int senderUserId)
        {
            if (serverInviteQueryableRepository.GetQueryable().Any(si => si.ServerId == serverId && si.RecieverUserId == recieverUserId))
                return new Result(false, "Invite already exists.");

            if (!serverQueryableRepository.GetQueryable().Any(s => s.Id == serverId))
                return new Result(false, "Server not found.");

            if (!userQueryableRepository.GetQueryable().Any(u => u.Id == recieverUserId))
                return new Result(false, "Reciever not found.");

            if (!userQueryableRepository.GetQueryable().Any(u => u.Id == senderUserId))
                return new Result(false, "Sender not found.");

            var invite = new ServerInvite
            {
                ServerId = serverId,
                RecieverUserId = recieverUserId,
                SenderUserId = senderUserId
            };

            await serverInviteQueryableRepository.Create(invite);
            await callbackService.NotifyAsync(CallbackType.User, recieverUserId);

            return new Result(true);
        }

        public async Task<Result> DeleteInvite(int serverInviteId)
        {
            var invite = serverInviteQueryableRepository.GetQueryable().FirstOrDefault(si => si.Id == serverInviteId);
            if (invite is null)
                return new Result(false, "Invite not found.");

            var recieverUserId = invite.RecieverUserId;

            await serverInviteQueryableRepository.Delete(invite);
            await callbackService.NotifyAsync(CallbackType.User, recieverUserId);

            return new Result(true);
        }

        public async Task<Result> AcceptInvite(int serverInviteId)
        {
            var invite = serverInviteQueryableRepository.GetQueryable().FirstOrDefault(si => si.Id == serverInviteId);
            if (invite is null)
                return new Result(false, "Invite not found.");

            var server = serverQueryableRepository.GetQueryable().FirstOrDefault(s => s.Id == invite.ServerId);
            if (server is null)
                return new Result(false, "Server not found.");

            var reciever = userQueryableRepository.GetQueryable().FirstOrDefault(u => u.Id == invite.RecieverUserId);
            if (reciever is null)
                return new Result(false, "Reciever not found.");

            var userServer = new UserServer
            {
                UserId = invite.RecieverUserId,
                ServerId = invite.ServerId
            };

            await userServerQueryableRepository.Create(userServer);
            await serverInviteQueryableRepository.Delete(invite);

            await callbackService.NotifyAsync(CallbackType.User, userServer.UserId);
            await callbackService.NotifyAsync(CallbackType.Server, userServer.ServerId);

            return new Result(true);
        }
    }
}
