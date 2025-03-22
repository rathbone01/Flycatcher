﻿using Flycatcher.Classes;
using Flycatcher.Models.Database;
using Flycatcher.Models.Results;
using Flycatcher.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Flycatcher.Services
{
    public class ServerService
    {
        private readonly QueryableRepository queryableRepository;

        public ServerService(QueryableRepository queryableRepository)
        {
            this.queryableRepository = queryableRepository;
        }

        public int GetServerOwnerUserId(int serverId)
        {
            return queryableRepository
                .GetQueryable<Server>()
                .FirstOrDefault(s => s.Id == serverId)?.OwnerUserId ?? -1;
        }

        public string GetServerName(int serverId)
        {
            return queryableRepository
                .GetQueryable<Server>()
                .FirstOrDefault(s => s.Id == serverId)?.Name ?? "Error Loading Server Name";
        }

        public List<User> GetServerUsers(int serverId)
        {
            return queryableRepository
                .GetQueryable<UserServer>()
                .Where(us => us.ServerId == serverId)
                .Select(us => us.User)
                .ToList();
        }

        public List<Channel> GetServerChannels(int serverId)
        {
            var channels = queryableRepository
                .GetQueryable<Channel>()
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

            queryableRepository.Create(server);
            await queryableRepository.SaveChanges();

            var userServer = new UserServer
            {
                UserId = ownerId,
                ServerId = server.Id
            };

            queryableRepository.Create(userServer);
            await queryableRepository.SaveChanges();
        }

        public async Task<Result> DeleteServer(int serverId)
        {
            var server = queryableRepository
                .GetQueryable<Server>()
                .FirstOrDefault(s => s.Id == serverId);

            if (server is null)
                return new Result(false, "Server not found.");

            queryableRepository.Delete(server);
            await queryableRepository.SaveChanges();

            return new Result(true);
        }
    }
}
