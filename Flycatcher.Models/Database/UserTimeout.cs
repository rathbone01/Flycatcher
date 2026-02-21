using System.ComponentModel.DataAnnotations;

namespace Flycatcher.Models.Database
{
    public class UserTimeout
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ServerId { get; set; }
        public int TimeoutByUserId { get; set; }
        [MaxLength(500)]
        public string Reason { get; set; } = null!;
        public DateTime TimeoutAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAtUtc { get; set; }
        public bool IsActive => ExpiresAtUtc > DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; } = null!;
        public Server Server { get; set; } = null!;
        public User TimeoutByUser { get; set; } = null!;
    }
}
