using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Flycatcher.Services.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Flycatcher.Services
{
    public class UserRoleService
    {
        private readonly IQueryableRepository<UserRole> _userRoleRepository;
        private readonly IQueryableRepository<Role> _roleRepository;
        private readonly CallbackService _callbackService;

        public UserRoleService(IQueryableRepository<UserRole> userRoleRepository, IQueryableRepository<Role> roleRepository, CallbackService callbackService)
        {
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _callbackService = callbackService;
        }

        /// <summary>
        /// Get all UserRole assignments for a user in a server.
        /// Includes Role navigation property.
        /// </summary>
        public async Task<List<UserRole>> GetUserRolesAsync(int userId, int serverId)
        {
            return await _userRoleRepository
                .ExecuteAsync(q => q
                    .Include(ur => ur.Role)
                    .Where(ur => ur.UserId == userId && ur.Role.ServerId == serverId)
                    .ToListAsync());
        }

        /// <summary>
        /// Get count of roles a user has in a server.
        /// </summary>
        public async Task<int> GetUserRoleCountAsync(int userId, int serverId)
        {
            return await _userRoleRepository
                .ExecuteAsync(q => q
                    .Where(ur => ur.UserId == userId && ur.Role.ServerId == serverId)
                    .CountAsync());
        }

        /// <summary>
        /// Assign a role to a user. Returns error string on failure, null on success.
        /// Validates:
        ///   - Role must exist and belong to the server
        ///   - User cannot already have this role
        ///   - User cannot exceed 16 roles in the server
        /// After success, notifies UserRoleChanged for userId and RolesUpdated for serverId
        /// </summary>
        public async Task<string?> AssignRoleAsync(int userId, int roleId)
        {
            // Load the role to check if it exists and get its ServerId
            var role = await _roleRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(r => r.Id == roleId));

            if (role is null)
                return "Role not found.";

            // Check if user already has this role
            var existingUserRole = await _userRoleRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId));

            if (existingUserRole is not null)
                return "User already has this role.";

            // Check if user has reached the maximum role limit in this server
            var userRoleCount = await GetUserRoleCountAsync(userId, role.ServerId);

            if (userRoleCount >= 16)
                return "User cannot have more than 16 roles in a server.";

            // Create the new UserRole assignment
            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                AssignedAtUtc = DateTime.UtcNow
            };

            await _userRoleRepository.Create(userRole);

            // Notify subscribers of the changes
            await _callbackService.NotifyAsync(CallbackType.UserRoleChanged, CallbackIdGenerator.CreateId(CallbackType.UserRoleChanged, userId));
            await _callbackService.NotifyAsync(CallbackType.RolesUpdated, CallbackIdGenerator.CreateId(CallbackType.RolesUpdated, role.ServerId));

            return null;
        }

        /// <summary>
        /// Remove a role from a user. Returns error string on failure, null on success.
        /// After success, notifies UserRoleChanged for userId and RolesUpdated for serverId
        /// </summary>
        public async Task<string?> RemoveRoleAsync(int userId, int roleId)
        {
            // Load the UserRole to verify it exists and get the Role navigation property
            var userRole = await _userRoleRepository
                .ExecuteAsync(q => q
                    .Include(ur => ur.Role)
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId));

            if (userRole is null)
                return "User does not have this role.";

            var serverId = userRole.Role.ServerId;

            await _userRoleRepository.Delete(userRole);

            // Notify subscribers of the changes
            await _callbackService.NotifyAsync(CallbackType.UserRoleChanged, CallbackIdGenerator.CreateId(CallbackType.UserRoleChanged, userId));
            await _callbackService.NotifyAsync(CallbackType.RolesUpdated, CallbackIdGenerator.CreateId(CallbackType.RolesUpdated, serverId));

            return null;
        }

        /// <summary>
        /// Bulk-set roles for a user in a server: replaces all current roles with newRoleIds.
        /// Validates: all roleIds must belong to serverId, count must not exceed 16.
        /// Returns error string on failure, null on success.
        /// After success, notifies callbacks.
        /// </summary>
        public async Task<string?> SetUserRolesAsync(int userId, int serverId, List<int> newRoleIds)
        {
            // Validate role count
            if (newRoleIds.Count > 16)
                return "User cannot have more than 16 roles in a server.";

            // If there are new roles to assign, validate they all belong to the correct server
            if (newRoleIds.Count > 0)
            {
                var roles = await _roleRepository
                    .ExecuteAsync(q => q
                        .Where(r => newRoleIds.Contains(r.Id))
                        .ToListAsync());

                // Check if all roles exist
                if (roles.Count != newRoleIds.Count)
                    return "One or more roles not found.";

                // Check if all roles belong to the correct server
                if (roles.Any(r => r.ServerId != serverId))
                    return "All roles must belong to the same server.";
            }

            // Remove all existing roles for this user in this server
            await _userRoleRepository.ExecuteDelete(ur => ur.UserId == userId && ur.Role.ServerId == serverId);

            // Create new UserRole assignments
            foreach (var roleId in newRoleIds)
            {
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    AssignedAtUtc = DateTime.UtcNow
                };

                await _userRoleRepository.Create(userRole);
            }

            // Notify subscribers of the changes
            await _callbackService.NotifyAsync(CallbackType.UserRoleChanged, CallbackIdGenerator.CreateId(CallbackType.UserRoleChanged, userId));
            await _callbackService.NotifyAsync(CallbackType.RolesUpdated, CallbackIdGenerator.CreateId(CallbackType.RolesUpdated, serverId));

            return null;
        }

        /// <summary>
        /// Check if a user has a specific role.
        /// </summary>
        public async Task<bool> UserHasRoleAsync(int userId, int roleId)
        {
            return await _userRoleRepository
                .ExecuteAsync(q => q.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId));
        }
    }
}
