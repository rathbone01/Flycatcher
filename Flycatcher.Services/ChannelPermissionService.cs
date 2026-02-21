using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Flycatcher.Services
{
    public class ChannelPermissionService
    {
        private readonly IQueryableRepository<ChannelRolePermission> _channelRolePermissionRepository;

        public ChannelPermissionService(IQueryableRepository<ChannelRolePermission> channelRolePermissionRepository)
        {
            _channelRolePermissionRepository = channelRolePermissionRepository;
        }

        /// <summary>
        /// Get all channel role permission overrides for a channel.
        /// </summary>
        public async Task<List<ChannelRolePermission>> GetChannelOverridesAsync(int channelId)
        {
            var overrides = await _channelRolePermissionRepository
                .ExecuteAsync(q => q.Where(crp => crp.ChannelId == channelId).ToListAsync());

            return overrides;
        }

        /// <summary>
        /// Get the override for a specific role in a channel (null if none exists).
        /// </summary>
        public async Task<ChannelRolePermission?> GetChannelRoleOverrideAsync(int channelId, int roleId)
        {
            var channelOverride = await _channelRolePermissionRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(crp => crp.ChannelId == channelId && crp.RoleId == roleId));

            return channelOverride;
        }

        /// <summary>
        /// Set or update a channel-level permission override for a role.
        /// If no override exists, create one. If exists, update it.
        /// sendMessages: null = inherit, true = allow, false = deny
        /// deleteOthersMessages: null = inherit, true = allow, false = deny
        /// Returns error string on failure, null on success.
        /// </summary>
        public async Task<string?> SetChannelOverrideAsync(int channelId, int roleId, bool? sendMessages, bool? deleteOthersMessages)
        {
            var existingOverride = await _channelRolePermissionRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(crp => crp.ChannelId == channelId && crp.RoleId == roleId));

            if (existingOverride is null)
            {
                var newOverride = new ChannelRolePermission
                {
                    ChannelId = channelId,
                    RoleId = roleId,
                    SendMessages = sendMessages,
                    DeleteOthersMessages = deleteOthersMessages
                };

                await _channelRolePermissionRepository.Create(newOverride);
            }
            else
            {
                existingOverride.SendMessages = sendMessages;
                existingOverride.DeleteOthersMessages = deleteOthersMessages;

                await _channelRolePermissionRepository.Update(existingOverride);
            }

            return null;
        }

        /// <summary>
        /// Remove a channel override (reset to inherit from role).
        /// </summary>
        public async Task<string?> RemoveChannelOverrideAsync(int channelId, int roleId)
        {
            var existingOverride = await _channelRolePermissionRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(crp => crp.ChannelId == channelId && crp.RoleId == roleId));

            if (existingOverride is null)
                return "Channel override not found.";

            await _channelRolePermissionRepository.Delete(existingOverride);

            return null;
        }
    }
}
