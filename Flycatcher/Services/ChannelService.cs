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

        public ChannelService(QueryableRepository queryableRepository)
        {
            this.queryableRepository = queryableRepository;
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
        }

        public async Task<Result> DeleteChannel(int channelId)
        {
            var channel = queryableRepository
                .GetQueryable<Channel>()
                .FirstOrDefault(c => c.Id == channelId);

            if (channel is null)
                return new Result(false, "Channel not found.");

            queryableRepository.Delete(channel);
            await queryableRepository.SaveChangesAsync();

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
