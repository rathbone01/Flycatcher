namespace Flycatcher.Models.Database
{
    /// <summary>
    /// Channel-level permission overrides for a specific role.
    /// Each permission is a nullable bool:
    ///   null  = inherit from role's server-level permission
    ///   true  = explicitly allowed in this channel
    ///   false = explicitly denied in this channel
    /// Add new permissions by adding new nullable bool properties.
    /// </summary>
    public class ChannelRolePermission
    {
        public int Id { get; set; }
        public int ChannelId { get; set; }
        public int RoleId { get; set; }

        // Messaging
        public bool? SendMessages { get; set; }

        // Moderation
        public bool? DeleteOthersMessages { get; set; }

        // Navigation properties
        public Channel Channel { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}
