using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Flycatcher.Models.Results;
using Microsoft.EntityFrameworkCore;
using Flycatcher.Services.Enumerations;

namespace Flycatcher.Services
{
    public class ServerService
    {
        private readonly IQueryableRepository<Server> serverQueryableRepository;
        private readonly IQueryableRepository<UserServer> userServerQueryableRepository;
        private readonly IQueryableRepository<Channel> channelQueryableRepository;
        private readonly IQueryableRepository<Message> messageQueryableRepository;
        private readonly CallbackService callbackService;

        public ServerService(IQueryableRepository<Server> serverQueryableRepository, IQueryableRepository<UserServer> userServerQueryableRepository,  CallbackService callbackService, IQueryableRepository<Channel> channelQueryableRepository, IQueryableRepository<Message> messageQueryableRepository)
        {
            this.serverQueryableRepository = serverQueryableRepository;
            this.callbackService = callbackService;
            this.userServerQueryableRepository = userServerQueryableRepository;
            this.channelQueryableRepository = channelQueryableRepository;
            this.messageQueryableRepository = messageQueryableRepository;
        }

        public async Task<bool> DoesServerExist(int serverId)
        {
            return await serverQueryableRepository
                .GetQueryable()
                .AnyAsync(s => s.Id == serverId);
        }

        public async Task<int> GetServerOwnerUserId(int serverId)
        {

            var server = await serverQueryableRepository
            .GetQueryable()
            .FirstOrDefaultAsync(s => s.Id == serverId);
                
            if (server is null)
                return -1;

            return server.OwnerUserId;
        }

        public async Task<string> GetServerName(int serverId)
        {
            var server = await serverQueryableRepository
                 .GetQueryable()
                 .FirstOrDefaultAsync(s => s.Id == serverId);

            if (server is null)
                return string.Empty;

            return server.Name;
        }

        public async  Task<List<User>> GetServerUsers(int serverId)
        {
            return await userServerQueryableRepository
                .GetQueryable()
                .Where(us => us.ServerId == serverId)
                .Select(us => us.User)
                .ToListAsync();
        }

        public async Task<List<Channel>> GetServerChannels(int serverId)
        {
            return await channelQueryableRepository
                .GetQueryable()
                .Where(c => c.ServerId == serverId)
                .ToListAsync();
        }

        public async Task CreateServer(string serverName, int ownerId)
        {
            var server = new Server
            {
                Name = serverName,
                OwnerUserId = ownerId
            };

            await serverQueryableRepository.Create(server);

            var userServer = new UserServer
            {
                UserId = ownerId,
                ServerId = server.Id
            };

            await userServerQueryableRepository.Create(userServer);
            await callbackService.NotifyAsync(CallbackType.UserServerListUpdated, server.Id);
        }

        public async Task<Result> DeleteServer(int serverId)
        {
            var server = await serverQueryableRepository
                .GetQueryable()
                .FirstOrDefaultAsync(s => s.Id == serverId);

            if (server is null)
                return new Result(false, "Server not found.");

            //delete all channels, and messages in those channels
            var channels = await channelQueryableRepository
                .GetQueryable()
                .Where(c => c.ServerId == serverId)
                .ToListAsync();

            foreach (var channel in channels)
            {
                await messageQueryableRepository.ExecuteDelete(m => m.ChannelId == channel.Id);
            }

            await channelQueryableRepository.ExecuteDelete(c => c.ServerId == serverId);
            await serverQueryableRepository.Delete(server);
            await callbackService.NotifyAsync(CallbackType.ServerDeleted, serverId);

            return new Result(true);
        }

        public async Task<Result> LeaveServer(int userId, int serverId)
        {
            var userServer = await userServerQueryableRepository
                .GetQueryable()
                .FirstOrDefaultAsync(us => us.UserId == userId && us.ServerId == serverId);

            if (userServer is null)
                return new Result(false, "User not in server.");

            await userServerQueryableRepository.Delete(userServer);
            await callbackService.NotifyAsync(CallbackType.UserServerListUpdated, userServer.UserId);
            await callbackService.NotifyAsync(CallbackType.ServerUserUpdated, serverId);

            return new Result(true);
        }
    }
}
