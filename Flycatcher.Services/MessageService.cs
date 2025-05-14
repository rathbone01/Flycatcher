using Flycatcher.Classes;
using Flycatcher.DataAccess;
using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Flycatcher.Models.Results;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;

namespace Flycatcher.Services
{

    public class MessageService
    {
        private readonly IQueryableRepository queryableRepository;
        readonly CallbackService callbackService;

        public MessageService(IQueryableRepository queryableRepository, CallbackService callbackService)
        {
            this.queryableRepository = queryableRepository;
            this.callbackService = callbackService;
        }

        public int GetChannelMessagesCount(int channelId)
        {
            return queryableRepository
                .GetQueryable<Message>()
                .Count(m => m.ChannelId == channelId);
        }

        public List<Message> GetChannelMessages(int channelId, int startIndex, int count)
        {
            return queryableRepository
                .GetQueryable<Message>()
                .OrderByDescending(m => m.Timestamp)
                .Where(m => m.ChannelId == channelId)
                .Include(m => m.User)
                .Skip(startIndex)
                .Take(count)
                .ToList();
        }

        public async Task CreateMessage(int userId, int channelId, string content)
        {
            var message = new Message
            {
                UserId = userId,
                ChannelId = channelId,
                Content = content
            };

            queryableRepository.Create(message);
            await queryableRepository.SaveChangesAsync();
            await callbackService.NotifyAsync(CallbackType.Channel, channelId);
        }

        public async Task<Result> DeleteMessage(int messageId)
        {
            var message = queryableRepository
                .GetQueryable<Message>()
                .FirstOrDefault(m => m.Id == messageId);

            if (message is null)
                return new Result(false, "Message not found.");

            var channelId = message.ChannelId;

            queryableRepository.Delete(message);
            await queryableRepository.SaveChangesAsync();
            await callbackService.NotifyAsync(CallbackType.Channel, channelId);

            return new Result(true);
        }
    }
}
