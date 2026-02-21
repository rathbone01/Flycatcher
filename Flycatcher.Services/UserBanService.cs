using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Flycatcher.Services.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Flycatcher.Services
{
    public class UserBanService
    {
        private readonly IQueryableRepository<UserBan> userBanRepository;
        private readonly IQueryableRepository<SiteAdmin> siteAdminRepository;
        private readonly CallbackService callbackService;

        public UserBanService(
            IQueryableRepository<UserBan> userBanRepository,
            IQueryableRepository<SiteAdmin> siteAdminRepository,
            CallbackService callbackService)
        {
            this.userBanRepository = userBanRepository;
            this.siteAdminRepository = siteAdminRepository;
            this.callbackService = callbackService;
        }

        public async Task<bool> IsUserBannedAsync(int userId)
        {
            return await userBanRepository
                .ExecuteAsync(q => q.AnyAsync(ub => ub.UserId == userId));
        }

        public async Task<UserBan?> GetUserBanAsync(int userId)
        {
            return await userBanRepository
                .ExecuteAsync(q => q
                    .Include(ub => ub.User)
                    .Include(ub => ub.BannedByAdmin)
                    .Include(ub => ub.AppealReviewedByAdmin)
                    .FirstOrDefaultAsync(ub => ub.UserId == userId));
        }

        public async Task<string?> BanUserAsync(int userId, int adminUserId, string reason)
        {
            if (userId == adminUserId)
                return "You cannot ban yourself.";

            var targetIsSiteAdmin = await siteAdminRepository
                .ExecuteAsync(q => q.AnyAsync(sa => sa.UserId == userId));
            if (targetIsSiteAdmin)
                return "Site administrators cannot be banned.";

            var existingBan = await userBanRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(ub => ub.UserId == userId));

            if (existingBan is not null)
                return $"User is already banned. Banned on {existingBan.BannedAtUtc:yyyy-MM-dd} for: {existingBan.Reason}";

            var userBan = new UserBan
            {
                UserId = userId,
                BannedByAdminUserId = adminUserId,
                Reason = reason,
                BannedAtUtc = DateTime.UtcNow,
                AppealStatus = AppealStatus.None
            };

            await userBanRepository.Create(userBan);
            await callbackService.NotifyAsync(CallbackType.UserBanned, CallbackIdGenerator.CreateId(CallbackType.UserBanned, userId));

            return null;
        }

        public async Task<string?> SubmitAppealAsync(int userId, string appealReason)
        {
            var userBan = await userBanRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(ub => ub.UserId == userId));

            if (userBan is null)
                return "User is not banned.";

            if (userBan.AppealStatus != AppealStatus.None)
                return "An appeal has already been submitted for this ban.";

            if (string.IsNullOrWhiteSpace(appealReason))
                return "Appeal reason cannot be empty.";

            if (appealReason.Length < 20)
                return "Appeal reason must be at least 20 characters long.";

            if (appealReason.Length > 1000)
                return "Appeal reason must not exceed 1000 characters.";

            userBan.AppealReason = appealReason;
            userBan.AppealStatus = AppealStatus.Pending;
            userBan.AppealSubmittedAtUtc = DateTime.UtcNow;

            await userBanRepository.Update(userBan);
            await callbackService.NotifyAsync(CallbackType.AppealSubmitted, CallbackIdGenerator.CreateAdminNotificationId());

            return null;
        }

        public async Task<List<UserBan>> GetPendingAppealsAsync()
        {
            return await userBanRepository
                .ExecuteAsync(q => q
                    .Where(ub => ub.AppealStatus == AppealStatus.Pending)
                    .Include(ub => ub.User)
                    .Include(ub => ub.BannedByAdmin)
                    .OrderBy(ub => ub.AppealSubmittedAtUtc)
                    .ToListAsync());
        }

        public async Task<List<UserBan>> GetAllBansAsync()
        {
            return await userBanRepository
                .ExecuteAsync(q => q
                    .Include(ub => ub.User)
                    .Include(ub => ub.BannedByAdmin)
                    .Include(ub => ub.AppealReviewedByAdmin)
                    .OrderByDescending(ub => ub.BannedAtUtc)
                    .ToListAsync());
        }

        public async Task<string?> ReviewAppealAsync(int banId, int adminUserId, bool approve)
        {
            var userBan = await userBanRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(ub => ub.Id == banId));

            if (userBan is null)
                return "Ban not found.";

            if (userBan.AppealStatus != AppealStatus.Pending)
                return "This appeal is not pending review.";

            if (approve)
            {
                await userBanRepository.Delete(userBan);
                await callbackService.NotifyAsync(CallbackType.AppealReviewed, CallbackIdGenerator.CreateId(CallbackType.AppealReviewed, userBan.UserId));
            }
            else
            {
                userBan.AppealStatus = AppealStatus.Denied;
                userBan.AppealReviewedAtUtc = DateTime.UtcNow;
                userBan.AppealReviewedByAdminUserId = adminUserId;

                await userBanRepository.Update(userBan);
                await callbackService.NotifyAsync(CallbackType.AppealReviewed, CallbackIdGenerator.CreateId(CallbackType.AppealReviewed, userBan.UserId));
            }

            return null;
        }

        public async Task<int> GetPendingAppealCountAsync()
        {
            return await userBanRepository
                .ExecuteAsync(q => q.CountAsync(ub => ub.AppealStatus == AppealStatus.Pending));
        }
    }
}
