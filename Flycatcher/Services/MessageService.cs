﻿using Flycatcher.Classes;
using Flycatcher.Models.Database;
using Flycatcher.Models.Results;
using Flycatcher.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Flycatcher.Services
{

    public class MessageService
    {
        private readonly QueryableRepository queryableRepository;

        public MessageService(QueryableRepository queryableRepository)
        {
            this.queryableRepository = queryableRepository;
        }

        public List<Message> GetChannelMessages(int channelId, int page)
        {
            return queryableRepository
                .GetQueryable<Message>()
                .OrderBy(m => m.Timestamp)
                .Where(m => m.ChannelId == channelId)
                .Include(m => m.User)
                .Skip(page * 100)
                .Take(100)
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
            await queryableRepository.SaveChanges();
        }

        public async Task<Result> DeleteMessage(int messageId)
        {
            var message = queryableRepository
                .GetQueryable<Message>()
                .FirstOrDefault(m => m.Id == messageId);

            if (message is null)
                return new Result(false, "Message not found.");

            queryableRepository.Delete(message);
            await queryableRepository.SaveChanges();

            return new Result(true);
        }
    }
}
