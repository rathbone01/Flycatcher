using Flycatcher.Classes;
using Flycatcher.Models.Database;
using Flycatcher.Models.Results;
using Flycatcher.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Flycatcher.Services
{
    public class ChannelService
    {
        private readonly QueryableRepository queryableRepository;
        private readonly CallbackService callbackService;

        public ChannelService(QueryableRepository queryableRepository, CallbackService callbackService)
        {
            this.queryableRepository = queryableRepository;
            this.callbackService = callbackService;
        }

        public async Task CreateChannel(string channelName, int serverId)
        {
            var channel = new Channel
            {
                Name = channelName,
                ServerId = serverId
            };

            queryableRepository.Create(channel);
            await queryableRepository.SaveChangesAsync();
            await callbackService.NotifyAsync(CallbackType.Server, serverId);
        }

        public async Task<Result> DeleteChannel(int channelId)
        {
            var channel = queryableRepository
                .GetQueryable<Channel>()
                .FirstOrDefault(c => c.Id == channelId);

            if (channel is null)
                return new Result(false, "Channel not found.");

            var serverId = channel.ServerId;

            queryableRepository.Delete(channel);
            await queryableRepository.SaveChangesAsync();
            await callbackService.NotifyAsync(CallbackType.Server, serverId);

            return new Result(true);
        }

        public string GetChannelName(int channelId)
        {
            return queryableRepository
                .GetQueryable<Channel>()
                .FirstOrDefault(c => c.Id == channelId)?.Name ?? "Error Loading Channel Name";
        }
    }
}
