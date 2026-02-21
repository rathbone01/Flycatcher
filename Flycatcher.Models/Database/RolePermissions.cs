namespace Flycatcher.Models.Database
{
    /// <summary>
    /// Stores the permissions for a Role as individual boolean columns.
    /// Each permission is a separate column for easy extensibility -
    /// adding new permissions requires only adding a new bool property and a migration.
    /// </summary>
    public class RolePermissions
    {
        public int Id { get; set; }
        public int RoleId { get; set; }

        // Messaging
        public bool SendMessages { get; set; }

        // Moderation
        public bool DeleteOthersMessages { get; set; }
        public bool TimeoutUser { get; set; }
        public bool BanUser { get; set; }

        // Channel management
        public bool EditChannels { get; set; }
        public bool AddChannels { get; set; }

        // Server management
        public bool EditServerSettings { get; set; }

        // Role management
        public bool ManageRoles { get; set; }
        public bool AssignRoles { get; set; }

        // Navigation property
        public Role Role { get; set; } = null!;
    }
}
