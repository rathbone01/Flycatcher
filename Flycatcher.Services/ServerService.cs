using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Flycatcher.Models.Results;

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

        public bool DoesServerExist(int serverId)
        {
            return serverQueryableRepository
                .GetQueryable()
                .Any(s => s.Id == serverId);
        }

        public int GetServerOwnerUserId(int serverId)
        {
            return serverQueryableRepository
                .GetQueryable()
                .FirstOrDefault(s => s.Id == serverId)?.OwnerUserId ?? -1;
        }

        public string GetServerName(int serverId)
        {
            return serverQueryableRepository
                .GetQueryable()
                .FirstOrDefault(s => s.Id == serverId)?.Name ?? "Error Loading Server Name";
        }

        public List<User> GetServerUsers(int serverId)
        {
            return userServerQueryableRepository
                .GetQueryable()
                .Where(us => us.ServerId == serverId)
                .Select(us => us.User)
                .ToList();
        }

        public List<Channel> GetServerChannels(int serverId)
        {
            var channels = channelQueryableRepository
                .GetQueryable()
                .Where(c => c.ServerId == serverId)
                .ToList();

            return channels;
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
        }

        public async Task<Result> DeleteServer(int serverId)
        {
            var server = serverQueryableRepository
                .GetQueryable()
                .FirstOrDefault(s => s.Id == serverId);

            if (server is null)
                return new Result(false, "Server not found.");

            //delete all channels, and messages in those channels
            var channels = channelQueryableRepository
                .GetQueryable()
                .Where(c => c.ServerId == serverId)
                .ToList();

            foreach (var channel in channels)
            {
                await messageQueryableRepository.ExecuteDelete(m => m.ChannelId == channel.Id);
            }

            await channelQueryableRepository.ExecuteDelete(c => c.ServerId == serverId);
            await serverQueryableRepository.Delete(server);
            await callbackService.NotifyAsync(CallbackType.Server, serverId);

            return new Result(true);
        }

        public async Task<Result> LeaveServer(int userId, int serverId)
        {
            var userServer = userServerQueryableRepository
                .GetQueryable()
                .FirstOrDefault(us => us.UserId == userId && us.ServerId == serverId);

            if (userServer is null)
                return new Result(false, "User not in server.");

            await userServerQueryableRepository.Delete(userServer);
            return new Result(true);
        }
    }
}
