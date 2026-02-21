namespace Flycatcher.Services.Enumerations
{
    /// <summary>
    /// Permission name constants used throughout the application.
    /// These strings correspond to bool properties on RolePermissions and ChannelRolePermission.
    /// To add a new permission:
    ///   1. Add a constant here
    ///   2. Add a bool property to RolePermissions
    ///   3. Add a bool? property to ChannelRolePermission (if it can be channel-overridden)
    ///   4. Add a migration
    /// </summary>
    public static class PermissionNames
    {
        // Messaging
        public const string SendMessages = nameof(SendMessages);

        // Moderation
        public const string DeleteOthersMessages = nameof(DeleteOthersMessages);
        public const string TimeoutUser = nameof(TimeoutUser);
        public const string BanUser = nameof(BanUser);

        // Channel management
        public const string EditChannels = nameof(EditChannels);
        public const string AddChannels = nameof(AddChannels);

        // Server management
        public const string EditServerSettings = nameof(EditServerSettings);

        // Role management
        public const string ManageRoles = nameof(ManageRoles);
        public const string AssignRoles = nameof(AssignRoles);

        /// <summary>Returns all permission name constants.</summary>
        public static IReadOnlyList<string> All => new[]
        {
            SendMessages,
            DeleteOthersMessages,
            TimeoutUser,
            BanUser,
            EditChannels,
            AddChannels,
            EditServerSettings,
            ManageRoles,
            AssignRoles
        };
    }
}
