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

        // Produces a stable, symmetric Guid key for a user pair used with CallbackService.
        // Delegates to CallbackIdGenerator for deterministic UUID v5 generation.
        public static Guid GetConversationKey(int userId1, int userId2)
        {
            return CallbackIdGenerator.CreateConversationId(userId1, userId2);
        }

        public async Task<int> GetDirectMessagesCount(int userId1, int userId2)
        {
            return await directMessageRepository
                .ExecuteAsync(q => q.CountAsync(dm =>
                    (dm.SenderId == userId1 && dm.ReceiverId == userId2) ||
                    (dm.SenderId == userId2 && dm.ReceiverId == userId1)));
        }

        // Returns messages projected to the Message view model so that MessageWidget can be reused.
        // The projection to Message is done in-memory after the EF query completes.
        public async Task<List<Message>> GetDirectMessages(int userId1, int userId2, int startIndex, int count)
        {
            var directMessages = await directMessageRepository
                .ExecuteAsync(q => q
                    .Where(dm =>
                        (dm.SenderId == userId1 && dm.ReceiverId == userId2) ||
                        (dm.SenderId == userId2 && dm.ReceiverId == userId1))
                    .OrderByDescending(dm => dm.Timestamp)
                    .Include(dm => dm.Sender)
                    .Skip(startIndex)
                    .Take(count)
                    .ToListAsync());

            return directMessages.Select(dm => new Message
            {
                Id = dm.Id,
                UserId = dm.SenderId,
                ChannelId = 0,
                Content = dm.Message,
                Timestamp = dm.Timestamp,
                User = dm.Sender
            }).ToList();
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
