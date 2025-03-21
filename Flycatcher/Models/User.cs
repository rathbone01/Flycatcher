using System.ComponentModel.DataAnnotations;

namespace Flycatcher.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        // Navigation properties
        public List<UserServer> UserServers { get; set; } = null!;
        public List<UserFriend> UserFriends { get; set; } = null!;
        public List<Message> Messages { get; set; } = null!;
    }
}
