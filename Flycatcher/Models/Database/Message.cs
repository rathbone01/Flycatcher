using System.ComponentModel.DataAnnotations;

namespace Flycatcher.Models.Database
{
    public class Message
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ChannelId { get; set; }
        public string Content { get; set; } = null!;

        // Navigation properties
        public User User { get; set; } = null!;
        public Channel Channel { get; set; } = null!;
    }
}
