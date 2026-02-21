using System.ComponentModel.DataAnnotations;

namespace Flycatcher.Models.Database
{
    public enum ReportStatus
    {
        Open = 0,
        Reviewed = 1,
        Dismissed = 2
    }

    public class UserReport
    {
        public int Id { get; set; }
        public int ReportedUserId { get; set; }
        public int? ReporterUserId { get; set; }

        [MaxLength(500)]
        public string Reason { get; set; } = null!;

        public ReportStatus Status { get; set; } = ReportStatus.Open;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAtUtc { get; set; }
        public int? ReviewedByAdminUserId { get; set; }

        // Navigation properties
        public User ReportedUser { get; set; } = null!;
        public User? Reporter { get; set; }
        public User? ReviewedByAdmin { get; set; }
    }
}
