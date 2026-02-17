using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Flycatcher.Services.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Flycatcher.Services
{
    public class DirectMessageService
    {
        private readonly IQueryableRepository<DirectMessage> directMessageRepository;
        private readonly CallbackService callbackService;

        public DirectMessageService(IQueryableRepository<DirectMessage> directMessageRepository, CallbackService callbackService)
        {
            this.directMessageRepository = directMessageRepository;
            this.callbackService = callbackService;
        }

        // Produces a stable, symmetric integer key for a user pair used with CallbackService.
        // Math.Min * 100000 + Math.Max guarantees the same value regardless of argument order.
        public static int GetConversationKey(int userId1, int userId2)
        {
            return Math.Min(userId1, userId2) * 100000 + Math.Max(userId1, userId2);
        }

        public async Task<int> GetDirectMessagesCount(int userId1, int userId2)
        {
            return await directMessageRepository
                .GetQueryable()
                .CountAsync(dm =>
                    (dm.SenderId == userId1 && dm.ReceiverId == userId2) ||
                    (dm.SenderId == userId2 && dm.ReceiverId == userId1));
        }

        // Returns messages projected to the Message view model so that MessageWidget can be reused.
        public async Task<List<Message>> GetDirectMessages(int userId1, int userId2, int startIndex, int count)
        {
            return await directMessageRepository
                .GetQueryable()
                .Where(dm =>
                    (dm.SenderId == userId1 && dm.ReceiverId == userId2) ||
                    (dm.SenderId == userId2 && dm.ReceiverId == userId1))
                .OrderByDescending(dm => dm.Timestamp)
                .Include(dm => dm.Sender)
                .Skip(startIndex)
                .Take(count)
                .Select(dm => new Message
                {
                    Id = dm.Id,
                    UserId = dm.SenderId,
                    ChannelId = 0,
                    Content = dm.Message,
                    Timestamp = dm.Timestamp,
                    User = dm.Sender
                })
                .ToListAsync();
        }

        public async Task CreateDirectMessage(int senderId, int receiverId, string content)
        {
            var dm = new DirectMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = content,
                Timestamp = DateTime.UtcNow
            };

            await directMessageRepository.Create(dm);
            await callbackService.NotifyAsync(CallbackType.DirectMessageEvent, GetConversationKey(senderId, receiverId));
        }
    }
}
