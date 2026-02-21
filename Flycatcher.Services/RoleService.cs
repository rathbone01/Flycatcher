using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Microsoft.EntityFrameworkCore;
using Flycatcher.Services.Enumerations;
using System.Text.RegularExpressions;

namespace Flycatcher.Services
{
    public class RoleService
    {
        private readonly IQueryableRepository<Role> roleRepository;
        private readonly IQueryableRepository<RolePermissions> rolePermissionsRepository;
        private readonly IQueryableRepository<UserRole> userRoleRepository;
        private readonly IQueryableRepository<ChannelRolePermission> channelRolePermissionRepository;
        private readonly CallbackService callbackService;

        public RoleService(
            IQueryableRepository<Role> roleRepository,
            IQueryableRepository<RolePermissions> rolePermissionsRepository,
            IQueryableRepository<UserRole> userRoleRepository,
            IQueryableRepository<ChannelRolePermission> channelRolePermissionRepository,
            CallbackService callbackService)
        {
            this.roleRepository = roleRepository;
            this.rolePermissionsRepository = rolePermissionsRepository;
            this.userRoleRepository = userRoleRepository;
            this.channelRolePermissionRepository = channelRolePermissionRepository;
            this.callbackService = callbackService;
        }

        public async Task<List<Role>> GetServerRolesAsync(int serverId)
        {
            return await roleRepository
                .ExecuteAsync(q => q
                    .Where(r => r.ServerId == serverId)
                    .Include(r => r.Permissions)
                    .ToListAsync());
        }

        public async Task<Role?> GetRoleAsync(int roleId)
        {
            return await roleRepository
                .ExecuteAsync(q => q
                    .Include(r => r.Permissions)
                    .FirstOrDefaultAsync(r => r.Id == roleId));
        }

        public async Task<(string? error, Role? role)> CreateRoleAsync(int serverId, string name, string? colorHex, int position)
        {
            // Validate name length
            if (string.IsNullOrWhiteSpace(name) || name.Length < 1 || name.Length > 32)
            {
                return ("Role name must be between 1 and 32 characters.", null);
            }

            // Validate color hex format if provided
            if (colorHex is not null && !IsValidHexColor(colorHex))
            {
                return ("Color must be in #RRGGBB format.", null);
            }

            var role = new Role
            {
                ServerId = serverId,
                Name = name,
                ColorHex = colorHex,
                Position = position,
                CreatedAtUtc = DateTime.UtcNow
            };

            await roleRepository.Create(role);

            var rolePermissions = new RolePermissions
            {
                RoleId = role.Id,
                SendMessages = false,
                DeleteOthersMessages = false,
                TimeoutUser = false,
                BanUser = false,
                EditChannels = false,
                AddChannels = false,
                EditServerSettings = false,
                ManageRoles = false,
                AssignRoles = false
            };

            await rolePermissionsRepository.Create(rolePermissions);
            await callbackService.NotifyAsync(CallbackType.RolesUpdated, CallbackIdGenerator.CreateId(CallbackType.RolesUpdated, serverId));

            return (null, role);
        }

        public async Task<string?> UpdateRoleAsync(int roleId, string name, string? colorHex, int position)
        {
            // Validate name length
            if (string.IsNullOrWhiteSpace(name) || name.Length < 1 || name.Length > 32)
            {
                return "Role name must be between 1 and 32 characters.";
            }

            // Validate color hex format if provided
            if (colorHex is not null && !IsValidHexColor(colorHex))
            {
                return "Color must be in #RRGGBB format.";
            }

            var role = await roleRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(r => r.Id == roleId));

            if (role is null)
            {
                return "Role not found.";
            }

            role.Name = name;
            role.ColorHex = colorHex;
            role.Position = position;

            await roleRepository.Update(role);
            await callbackService.NotifyAsync(CallbackType.RolesUpdated, CallbackIdGenerator.CreateId(CallbackType.RolesUpdated, role.ServerId));

            return null;
        }

        public async Task<string?> UpdateRolePermissionsAsync(int roleId, RolePermissions newPermissions)
        {
            var rolePermissions = await rolePermissionsRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(rp => rp.RoleId == roleId));

            if (rolePermissions is null)
            {
                return "Role permissions not found.";
            }

            rolePermissions.SendMessages = newPermissions.SendMessages;
            rolePermissions.DeleteOthersMessages = newPermissions.DeleteOthersMessages;
            rolePermissions.TimeoutUser = newPermissions.TimeoutUser;
            rolePermissions.BanUser = newPermissions.BanUser;
            rolePermissions.EditChannels = newPermissions.EditChannels;
            rolePermissions.AddChannels = newPermissions.AddChannels;
            rolePermissions.EditServerSettings = newPermissions.EditServerSettings;
            rolePermissions.ManageRoles = newPermissions.ManageRoles;
            rolePermissions.AssignRoles = newPermissions.AssignRoles;

            await rolePermissionsRepository.Update(rolePermissions);

            var role = await roleRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(r => r.Id == roleId));

            if (role is not null)
            {
                await callbackService.NotifyAsync(CallbackType.RolesUpdated, CallbackIdGenerator.CreateId(CallbackType.RolesUpdated, role.ServerId));
            }

            return null;
        }

        public async Task<string?> DeleteRoleAsync(int roleId)
        {
            var role = await roleRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(r => r.Id == roleId));

            if (role is null)
            {
                return "Role not found.";
            }

            var serverId = role.ServerId;

            // Delete all UserRole assignments for this role
            await userRoleRepository.ExecuteDelete(ur => ur.RoleId == roleId);

            // Delete all ChannelRolePermission overrides for this role (no cascade from Role FK)
            await channelRolePermissionRepository.ExecuteDelete(crp => crp.RoleId == roleId);

            // Delete the role itself (permissions will cascade delete)
            await roleRepository.Delete(role);
            await callbackService.NotifyAsync(CallbackType.RolesUpdated, CallbackIdGenerator.CreateId(CallbackType.RolesUpdated, serverId));

            return null;
        }

        public async Task<int> GetRoleUserCountAsync(int roleId)
        {
            return await userRoleRepository
                .ExecuteAsync(q => q.CountAsync(ur => ur.RoleId == roleId));
        }

        public async Task<Role?> GetUserPrimaryRoleAsync(int userId, int serverId)
        {
            return await userRoleRepository
                .ExecuteAsync(q => q
                    .Where(ur => ur.UserId == userId && ur.Role.ServerId == serverId)
                    .Include(ur => ur.Role)
                        .ThenInclude(r => r.Permissions)
                    .OrderByDescending(ur => ur.Role.Position)
                    .Select(ur => ur.Role)
                    .FirstOrDefaultAsync());
        }

        private static bool IsValidHexColor(string colorHex)
        {
            // Validate #RRGGBB format
            return Regex.IsMatch(colorHex, @"^#[0-9A-Fa-f]{6}$");
        }
    }
}
