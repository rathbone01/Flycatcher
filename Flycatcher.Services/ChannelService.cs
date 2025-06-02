using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Flycatcher.Models.Results;
using Microsoft.EntityFrameworkCore;
using Flycatcher.Services.Enumerations;

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
            await callbackService.NotifyAsync(CallbackType.ServerPropertyUpdated, serverId);
        }

        public async Task<Result> DeleteChannel(int channelId)
        {
            var channel = await channelQueryableRepository
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.Id == channelId);

            if (channel is null)
                return new Result(false, "Channel not found.");

            var serverId = channel.ServerId;

            // Delete all messages in the channel
            await messageQueryableRepository.ExecuteDelete(m => m.ChannelId == channelId);

            // Delete the channel itself
            await channelQueryableRepository.Delete(channel);
            await callbackService.NotifyAsync(CallbackType.ServerPropertyUpdated, serverId);

            return new Result(true);
        }

        public async Task<string> GetChannelName(int channelId)
        {
            var channel = await channelQueryableRepository
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.Id == channelId);

            if (channel is null)
                return string.Empty;

            return channel.Name;
        }
    }
}
