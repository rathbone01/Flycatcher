using Flycatcher.Classes;
using Flycatcher.DataAccess;
using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Flycatcher.Models.Results;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;
using Flycatcher.Services.Enumerations;

namespace Flycatcher.Services
{

    public class MessageService
    {
        private readonly IQueryableRepository<Message> messageQueryableRepository;
        readonly CallbackService callbackService;

        public MessageService(IQueryableRepository<Message> queryableRepository, CallbackService callbackService)
        {
            this.messageQueryableRepository = queryableRepository;
            this.callbackService = callbackService;
        }

        public async Task<int> GetChannelMessagesCount(int channelId)
        {
            return await messageQueryableRepository
                .ExecuteAsync(q => q.CountAsync(m => m.ChannelId == channelId));
        }

        public async Task<List<Message>> GetChannelMessages(int channelId, int startIndex, int count)
        {
            return await messageQueryableRepository
                .ExecuteAsync(q => q
                    .OrderByDescending(m => m.Timestamp)
                    .Where(m => m.ChannelId == channelId)
                    .Include(m => m.User)
                    .Skip(startIndex)
                    .Take(count)
                    .ToListAsync());
        }

        public async Task CreateMessage(int userId, int channelId, string content)
        {
            var message = new Message
            {
                UserId = userId,
                ChannelId = channelId,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            await messageQueryableRepository.Create(message);
            await callbackService.NotifyAsync(CallbackType.ChannelMessageEvent, CallbackIdGenerator.CreateId(CallbackType.ChannelMessageEvent, channelId));
        }

        public async Task<Result> DeleteMessage(int messageId)
        {
            var message = await messageQueryableRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(m => m.Id == messageId));

            if (message is null)
                return new Result(false, "Message not found.");

            var channelId = message.ChannelId;

            await messageQueryableRepository.Delete(message);
            await callbackService.NotifyAsync(CallbackType.ChannelMessageEvent, CallbackIdGenerator.CreateId(CallbackType.ChannelMessageEvent, channelId));

            return new Result(true);
        }
    }
}
