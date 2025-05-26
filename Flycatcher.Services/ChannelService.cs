using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Flycatcher.Models.Results;

namespace Flycatcher.Services
{
    public class ChannelService
    {
        private readonly IQueryableRepository<Channel> channelQueryableRepository;
        private readonly IQueryableRepository<Message> messageQueryableRepository;
        private readonly CallbackService callbackService;

        public ChannelService(IQueryableRepository<Channel> channelQueryableRepository, IQueryableRepository<Message> messageQueryableRepository, CallbackService callbackService)
        {
            this.channelQueryableRepository = channelQueryableRepository;
            this.messageQueryableRepository = messageQueryableRepository;
            this.callbackService = callbackService;
        }

        public async Task CreateChannel(string channelName, int serverId)
        {
            var channel = new Channel
            {
                Name = channelName,
                ServerId = serverId
            };

            await channelQueryableRepository.Create(channel);
            await callbackService.NotifyAsync(CallbackType.Server, serverId);
        }

        public async Task<Result> DeleteChannel(int channelId)
        {
            var channel = channelQueryableRepository
                .GetQueryable()
                .FirstOrDefault(c => c.Id == channelId);

            if (channel is null)
                return new Result(false, "Channel not found.");

            var serverId = channel.ServerId;

            // Delete all messages in the channel
            await messageQueryableRepository.ExecuteDelete(m => m.ChannelId == channelId);

            // Delete the channel itself
            await channelQueryableRepository.Delete(channel);
            await callbackService.NotifyAsync(CallbackType.Server, serverId);

            return new Result(true);
        }

        public string GetChannelName(int channelId)
        {
            return channelQueryableRepository
                .GetQueryable()
                .FirstOrDefault(c => c.Id == channelId)?.Name ?? "Error Loading Channel Name";
        }
    }
}
