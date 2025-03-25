using Flycatcher.Classes;
using Flycatcher.Models.Database;
using Flycatcher.Models.Results;
using Flycatcher.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Flycatcher.Services
{
    public class ServerInviteService
    {
        private readonly QueryableRepository queryableRepository;
        private readonly CallbackService callbackService;

        public ServerInviteService(QueryableRepository queryableRepository, CallbackService callbackService)
        {
            this.queryableRepository = queryableRepository;
            this.callbackService = callbackService;
        }

        public int GetServerInvitesCount(int userId)
        {
            return queryableRepository
                .GetQueryable<ServerInvite>()
                .Where(si => si.RecieverUserId == userId)
                .Include(si => si.Server)
                .Count();
        }

        public List<ServerInvite> GetServerInvites(int userId)
        {
            return queryableRepository
                .GetQueryable<ServerInvite>()
                .Where(si => si.RecieverUserId == userId)
                .Include(si => si.Server)
                .ToList();
        }

        public async Task<Result> CreateInvite(int serverId, int recieverUserId, int senderUserId)
        {
            if (queryableRepository.GetQueryable<ServerInvite>().Any(si => si.ServerId == serverId && si.RecieverUserId == recieverUserId))
                return new Result(false, "Invite already exists.");

            if (!queryableRepository.GetQueryable<Server>().Any(s => s.Id == serverId))
                return new Result(false, "Server not found.");

            if (!queryableRepository.GetQueryable<User>().Any(u => u.Id == recieverUserId))
                return new Result(false, "Reciever not found.");

            if (!queryableRepository.GetQueryable<User>().Any(u => u.Id == senderUserId))
                return new Result(false, "Sender not found.");

            var invite = new ServerInvite
            {
                ServerId = serverId,
                RecieverUserId = recieverUserId,
                SenderUserId = senderUserId
            };

            queryableRepository.Create(invite);
            await queryableRepository.SaveChangesAsync();
            await callbackService.NotifyAsync(CallbackType.User, recieverUserId);

            return new Result(true);
        }

        public async Task<Result> DeleteInvite(int serverInviteId)
        {
            var invite = queryableRepository.GetQueryable<ServerInvite>().FirstOrDefault(si => si.Id == serverInviteId);
            if (invite is null)
                return new Result(false, "Invite not found.");

            var recieverUserId = invite.RecieverUserId;

            queryableRepository.Delete(invite);
            await queryableRepository.SaveChangesAsync();
            await callbackService.NotifyAsync(CallbackType.User, recieverUserId);

            return new Result(true);
        }

        public async Task<Result> AcceptInvite(int serverInviteId)
        {
            var invite = queryableRepository.GetQueryable<ServerInvite>().FirstOrDefault(si => si.Id == serverInviteId);
            if (invite is null)
                return new Result(false, "Invite not found.");

            var server = queryableRepository.GetQueryable<Server>().FirstOrDefault(s => s.Id == invite.ServerId);
            if (server is null)
                return new Result(false, "Server not found.");

            var reciever = queryableRepository.GetQueryable<User>().FirstOrDefault(u => u.Id == invite.RecieverUserId);
            if (reciever is null)
                return new Result(false, "Reciever not found.");

            var userServer = new UserServer
            {
                UserId = invite.RecieverUserId,
                ServerId = invite.ServerId
            };

            queryableRepository.Create(userServer);
            queryableRepository.Delete(invite);
            await queryableRepository.SaveChangesAsync();
            await callbackService.NotifyAsync(CallbackType.User, userServer.UserId);

            return new Result(true);
        }
    }
}
