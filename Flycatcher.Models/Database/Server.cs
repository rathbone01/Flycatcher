using System.ComponentModel.DataAnnotations;

namespace Flycatcher.Models.Database
{
    public class Server
    {
        public int Id { get; set; }
        public int OwnerUserId { get; set; }
        public string Name { get; set; } = null!;

        // Navigation properties
        public List<Channel> Channels { get; set; } = null!;
        public List<UserServer> UserServers { get; set; } = null!;
    }
}
