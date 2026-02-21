using System.ComponentModel.DataAnnotations;

namespace Flycatcher.Models.Database
{
    public enum AppealStatus
    {
        None = 0,
        Pending = 1,
        Approved = 2,
        Denied = 3
    }

    public class UserBan
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BannedByAdminUserId { get; set; }

        [MaxLength(500)]
        public string Reason { get; set; } = null!;

        public DateTime BannedAtUtc { get; set; }
        public AppealStatus AppealStatus { get; set; } = AppealStatus.None;

        [MaxLength(1000)]
        public string? AppealReason { get; set; }

        public DateTime? AppealSubmittedAtUtc { get; set; }
        public DateTime? AppealReviewedAtUtc { get; set; }
        public int? AppealReviewedByAdminUserId { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public User BannedByAdmin { get; set; } = null!;
        public User? AppealReviewedByAdmin { get; set; }
    }
}
