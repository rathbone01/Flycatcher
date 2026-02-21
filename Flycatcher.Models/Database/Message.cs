using System.ComponentModel.DataAnnotations;

namespace Flycatcher.Models.Database
{
    public class Message
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ChannelId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAtUtc { get; set; }
        public int? DeletedByUserId { get; set; }
        public bool IsDeleted => DeletedAtUtc.HasValue;

        // Navigation properties
        public User User { get; set; } = null!;
        public Channel Channel { get; set; } = null!;
        public User? DeletedByUser { get; set; }
    }
}
