using System.ComponentModel.DataAnnotations;

namespace Flycatcher.Models.Database
{
    public class Role
    {
        public int Id { get; set; }
        public int ServerId { get; set; }
        [MaxLength(32)]
        public string Name { get; set; } = null!;
        public string? ColorHex { get; set; }
        public int Position { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        // Navigation properties
        public Server Server { get; set; } = null!;
        public RolePermissions? Permissions { get; set; }
        public List<UserRole> UserRoles { get; set; } = null!;
        public List<ChannelRolePermission> ChannelRolePermissions { get; set; } = null!;
    }
}
