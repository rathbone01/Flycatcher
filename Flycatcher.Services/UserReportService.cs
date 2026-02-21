using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Flycatcher.Services
{
    public class UserReportService
    {
        private readonly IQueryableRepository<UserReport> userReportRepository;
        private readonly IQueryableRepository<User> userRepository;

        public UserReportService(
            IQueryableRepository<UserReport> userReportRepository,
            IQueryableRepository<User> userRepository)
        {
            this.userReportRepository = userReportRepository;
            this.userRepository = userRepository;
        }

        public async Task<string?> ReportUserAsync(int reporterUserId, int reportedUserId, string reason)
        {
            // Validate: cannot report yourself
            if (reporterUserId == reportedUserId)
                return "You cannot report yourself.";

            // Validate: reason must be 10-500 chars
            if (string.IsNullOrWhiteSpace(reason))
                return "Reason is required.";

            var trimmedReason = reason.Trim();
            if (trimmedReason.Length < 10)
                return "Reason must be at least 10 characters.";

            if (trimmedReason.Length > 500)
                return "Reason must not exceed 500 characters.";

            // Validate: reported user exists
            var reportedUser = await userRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(u => u.Id == reportedUserId));

            if (reportedUser is null)
                return "Reported user not found.";

            // Validate: reporter exists
            var reporter = await userRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(u => u.Id == reporterUserId));

            if (reporter is null)
                return "Reporter user not found.";

            // Prevent duplicate open report for same reporter+reported within 24 hours
            var existingReport = await userReportRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(r =>
                    r.ReporterUserId == reporterUserId &&
                    r.ReportedUserId == reportedUserId &&
                    r.Status == ReportStatus.Open &&
                    r.CreatedAtUtc > DateTime.UtcNow.AddHours(-24)));

            if (existingReport != null)
                return "You have already submitted an open report for this user within the last 24 hours.";

            // Create the report
            var userReport = new UserReport
            {
                ReporterUserId = reporterUserId,
                ReportedUserId = reportedUserId,
                Reason = trimmedReason,
                Status = ReportStatus.Open,
                CreatedAtUtc = DateTime.UtcNow
            };

            await userReportRepository.Create(userReport);

            return null;
        }

        public async Task<List<UserReport>> GetAllReportsAsync()
        {
            return await userReportRepository
                .ExecuteAsync(q => q
                    .Include(r => r.ReportedUser)
                    .Include(r => r.Reporter)
                    .Include(r => r.ReviewedByAdmin)
                    .OrderByDescending(r => r.CreatedAtUtc)
                    .ToListAsync());
        }

        public async Task<List<UserReport>> GetPendingReportsAsync()
        {
            return await userReportRepository
                .ExecuteAsync(q => q
                    .Where(r => r.Status == ReportStatus.Open)
                    .Include(r => r.ReportedUser)
                    .Include(r => r.Reporter)
                    .OrderByDescending(r => r.CreatedAtUtc)
                    .ToListAsync());
        }

        public async Task<List<UserReport>> GetReportsByUserAsync(int reportedUserId)
        {
            return await userReportRepository
                .ExecuteAsync(q => q
                    .Where(r => r.ReportedUserId == reportedUserId)
                    .Include(r => r.ReportedUser)
                    .Include(r => r.Reporter)
                    .Include(r => r.ReviewedByAdmin)
                    .OrderByDescending(r => r.CreatedAtUtc)
                    .ToListAsync());
        }

        public async Task<string?> ReviewReportAsync(int reportId, int adminUserId, bool dismiss)
        {
            var report = await userReportRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(r => r.Id == reportId));

            if (report is null)
                return "Report not found.";

            if (report.Status != ReportStatus.Open)
                return "Report has already been reviewed.";

            var admin = await userRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(u => u.Id == adminUserId));

            if (admin is null)
                return "Admin user not found.";

            report.Status = dismiss ? ReportStatus.Dismissed : ReportStatus.Reviewed;
            report.ReviewedByAdminUserId = adminUserId;
            report.ReviewedAtUtc = DateTime.UtcNow;

            await userReportRepository.Update(report);

            return null;
        }

        public async Task<int> GetPendingReportCountAsync()
        {
            return await userReportRepository
                .ExecuteAsync(q => q.CountAsync(r => r.Status == ReportStatus.Open));
        }
    }
}
