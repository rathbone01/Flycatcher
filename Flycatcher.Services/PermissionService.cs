using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Flycatcher.Services.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Flycatcher.Services
{
    public class PermissionService
    {
        private readonly IQueryableRepository<Role> roleQueryableRepository;
        private readonly IQueryableRepository<UserRole> userRoleQueryableRepository;
        private readonly IQueryableRepository<RolePermissions> rolePermissionsQueryableRepository;
        private readonly IQueryableRepository<ChannelRolePermission> channelRolePermissionQueryableRepository;
        private readonly IQueryableRepository<Server> serverQueryableRepository;
        private readonly IQueryableRepository<SiteAdmin> siteAdminQueryableRepository;

        public PermissionService(IQueryableRepository<Role> roleQueryableRepository, IQueryableRepository<UserRole> userRoleQueryableRepository, IQueryableRepository<RolePermissions> rolePermissionsQueryableRepository, IQueryableRepository<ChannelRolePermission> channelRolePermissionQueryableRepository, IQueryableRepository<Server> serverQueryableRepository, IQueryableRepository<SiteAdmin> siteAdminQueryableRepository)
        {
            this.roleQueryableRepository = roleQueryableRepository;
            this.userRoleQueryableRepository = userRoleQueryableRepository;
            this.rolePermissionsQueryableRepository = rolePermissionsQueryableRepository;
            this.channelRolePermissionQueryableRepository = channelRolePermissionQueryableRepository;
            this.serverQueryableRepository = serverQueryableRepository;
            this.siteAdminQueryableRepository = siteAdminQueryableRepository;
        }

        public async Task<bool> IsSiteAdminAsync(int userId)
        {
            var siteAdmin = await siteAdminQueryableRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(sa => sa.UserId == userId));

            return siteAdmin != null;
        }

        public async Task<bool> IsServerOwnerAsync(int userId, int serverId)
        {
            var server = await serverQueryableRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(s => s.Id == serverId));

            if (server is null)
                return false;

            return server.OwnerUserId == userId;
        }

        public async Task<RolePermissions?> GetUserServerPermissionsAsync(int userId, int serverId)
        {
            if (await IsSiteAdminAsync(userId) || await IsServerOwnerAsync(userId, serverId))
            {
                return new RolePermissions
                {
                    SendMessages = true,
                    DeleteOthersMessages = true,
                    TimeoutUser = true,
                    BanUser = true,
                    EditChannels = true,
                    AddChannels = true,
                    EditServerSettings = true,
                    ManageRoles = true,
                    AssignRoles = true
                };
            }

            var roles = await GetUserRolesInServerAsync(userId, serverId);

            if (roles.Count == 0)
                return null;

            var combined = new RolePermissions();

            foreach (var role in roles)
            {
                if (role.Permissions is null)
                    continue;

                combined.SendMessages = combined.SendMessages || role.Permissions.SendMessages;
                combined.DeleteOthersMessages = combined.DeleteOthersMessages || role.Permissions.DeleteOthersMessages;
                combined.TimeoutUser = combined.TimeoutUser || role.Permissions.TimeoutUser;
                combined.BanUser = combined.BanUser || role.Permissions.BanUser;
                combined.EditChannels = combined.EditChannels || role.Permissions.EditChannels;
                combined.AddChannels = combined.AddChannels || role.Permissions.AddChannels;
                combined.EditServerSettings = combined.EditServerSettings || role.Permissions.EditServerSettings;
                combined.ManageRoles = combined.ManageRoles || role.Permissions.ManageRoles;
                combined.AssignRoles = combined.AssignRoles || role.Permissions.AssignRoles;
            }

            return combined;
        }

        public async Task<bool> HasServerPermissionAsync(int userId, int serverId, string permissionName)
        {
            if (await IsSiteAdminAsync(userId) || await IsServerOwnerAsync(userId, serverId))
                return true;

            var perms = await GetUserServerPermissionsAsync(userId, serverId);

            if (perms is null)
                return false;

            var value = typeof(RolePermissions).GetProperty(permissionName)?.GetValue(perms) as bool?;

            return value == true;
        }

        public async Task<bool> HasChannelPermissionAsync(int userId, int channelId, int serverId, string permissionName)
        {
            if (await IsSiteAdminAsync(userId) || await IsServerOwnerAsync(userId, serverId))
                return true;

            var roles = await GetUserRolesInServerAsync(userId, serverId);

            if (roles.Count == 0)
                return false;

            var roleIds = roles.Select(r => r.Id).ToList();

            var overrides = await channelRolePermissionQueryableRepository
                .ExecuteAsync(q => q
                    .Where(crp => crp.ChannelId == channelId && roleIds.Contains(crp.RoleId))
                    .ToListAsync());

            if (overrides.Count > 0)
            {
                bool hasExplicitAllow = false;
                bool hasExplicitDeny = false;
                bool allNull = true;

                foreach (var channelOverride in overrides)
                {
                    var value = typeof(ChannelRolePermission).GetProperty(permissionName)?.GetValue(channelOverride) as bool?;

                    if (value == true)
                    {
                        hasExplicitAllow = true;
                        allNull = false;
                    }
                    else if (value == false)
                    {
                        hasExplicitDeny = true;
                        allNull = false;
                    }
                }

                if (!allNull)
                {
                    if (hasExplicitAllow)
                        return true;

                    if (hasExplicitDeny)
                        return false;
                }
            }

            return await HasServerPermissionAsync(userId, serverId, permissionName);
        }

        public async Task<List<Role>> GetUserRolesInServerAsync(int userId, int serverId)
        {
            return await userRoleQueryableRepository
                .ExecuteAsync(q => q
                    .Where(ur => ur.UserId == userId)
                    .Include(ur => ur.Role)
                        .ThenInclude(r => r.Permissions)
                    .Where(ur => ur.Role.ServerId == serverId)
                    .Select(ur => ur.Role)
                    .ToListAsync());
        }
    }
}
