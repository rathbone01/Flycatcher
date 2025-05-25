using System.ComponentModel.DataAnnotations;

namespace Flycatcher.Models.Database
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime CreatedAtUtc { get; set; } = DateTime.MinValue;

        // Navigation properties
        public List<UserServer> UserServers { get; set; } = null!;
        public List<UserFriend> UserFriends { get; set; } = null!;
        public List<Message> Messages { get; set; } = null!;
    }
}
