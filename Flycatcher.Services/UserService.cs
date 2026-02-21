using Flycatcher.Classes;
using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Flycatcher.Models.Results;
using Microsoft.EntityFrameworkCore;
using Flycatcher.Services.Enumerations;

namespace Flycatcher.Services
{
    public class UserService
    {
        private readonly IQueryableRepository<User> userQueryableRepository;
        private readonly IQueryableRepository<SiteAdmin> siteAdminQueryableRepository;
        private readonly IQueryableRepository<UserServer> userServerQueryableRepository;
        private readonly IQueryableRepository<UserFriend> userFriendQueryableRepository;
        private readonly IQueryableRepository<FriendRequest> friendRequestQueryableRepository;
        private readonly IQueryableRepository<UserBan> userBanQueryableRepository;
        private readonly CallbackService callbackService;

        public UserService(IQueryableRepository<User> userQueryableRepository, IQueryableRepository<SiteAdmin> siteAdminQueryableRepository, IQueryableRepository<UserServer> userServerQueryableRepository, IQueryableRepository<UserFriend> userFriendQueryableRepository, IQueryableRepository<FriendRequest> friendRequestQueryableRepository, IQueryableRepository<UserBan> userBanQueryableRepository, CallbackService callbackService)
        {
            this.userQueryableRepository = userQueryableRepository;
            this.callbackService = callbackService;
            this.siteAdminQueryableRepository = siteAdminQueryableRepository;
            this.userServerQueryableRepository = userServerQueryableRepository;
            this.userFriendQueryableRepository = userFriendQueryableRepository;
            this.friendRequestQueryableRepository = friendRequestQueryableRepository;
            this.userBanQueryableRepository = userBanQueryableRepository;
        }

        public async Task<string> GetUsername(int userId)
        {
            var user = await userQueryableRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(u => u.Id == userId));

            if (user is null)
                return string.Empty;

            return user.Username;
        }

        public async Task<User?> GetUser(int userId)
        {
            return await userQueryableRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(u => u.Id == userId));
        }

        public async Task<User?> GetUser(string username)
        {
            return await userQueryableRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(u => u.Username == username));
        }

        public async Task<bool> IsUserSiteAdmin(int userId)
        {
            var siteAdmin = await siteAdminQueryableRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(sa => sa.UserId == userId));

            return siteAdmin != null;
        }

        public async Task<bool> IsUserGloballyBannedAsync(int userId)
        {
            var userBan = await userBanQueryableRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(ub => ub.UserId == userId));

            return userBan != null;
        }

        public async Task<LoginResult> Login(string username, string hashedPassword)
        {
            var user = await userQueryableRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(u => u.Username == username));

            if (user is null)
                return new LoginResult(false, "User not found.");

            if (user.PasswordHash != hashedPassword)
                return new LoginResult(false, "Incorrect password");

            // Allow banned users to log in so they can see the ban screen and submit appeals
            // The ban check is handled in Home.razor after login

            return new LoginResult(user.Id);
        }

        private LoginResult Login(User? user, string hashedPassword)
        {
            if (user is null)
                return new LoginResult(false, "User not found.");

            if (user.PasswordHash != hashedPassword)
                return new LoginResult(false, "Incorrect password");

            return new LoginResult(user.Id);
        }

        public async Task<Result> CreateUser(string username, string password, string email)
        {
            if (await userQueryableRepository.ExecuteAsync(q => q.AnyAsync(u => u.Username == username)))
                return new Result(false, "Username already exists.");

            if (await userQueryableRepository.ExecuteAsync(q => q.AnyAsync(u => u.Email == email)))
                return new Result(false, "Email already in use.");

            if (!IsValidEmail(email))
                return new Result(false, "Invalid email address. Please provide a properly formatted email.");

            if (!IsValidUsername(username))
                return new Result(false, "Invalid username. Username must be 3-20 characters long and can only contain letters, numbers, underscores, and periods.");

            if (!IsValidPassword(password))
                return new Result(false, "Invalid password. Password must be 8-64 characters long and include at least one uppercase letter, one lowercase letter, one digit, and one special character.");

            var user = new User
            {
                Username = username,
                PasswordHash = HashClass.Hash(password),
                Email = email.Trim().ToLower(),
                CreatedAtUtc = DateTime.UtcNow
            };

            await userQueryableRepository.Create(user);

            return new Result(true);
        }

        private static bool IsValidPassword(string password)
        {
#if DEBUG
            return true; // relax password requirements in debug mode for easier testing
#endif

            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < 8 || password.Length > 64)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecialChar = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasDigit && hasSpecialChar;
        }

        private static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            if (username.Length < 3 || username.Length > 20)
                return false;

            // Only allow letters, numbers, underscores, and periods
            return username.All(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '.');
        }

        private static bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Server>> GetUserServers(int userId)
        {
            return await userServerQueryableRepository
                .ExecuteAsync(q => q
                    .Where(us => us.UserId == userId)
                    .Select(us => us.Server)
                    .ToListAsync());
        }

        public async Task<List<User>> GetUserFriends(int userId)
        {
            var listUser = await userFriendQueryableRepository
                .ExecuteAsync(q => q
                    .Where(uf => uf.UserId == userId)
                    .Select(uf => uf.Friend)
                    .ToListAsync());

            listUser.AddRange(await userFriendQueryableRepository
                .ExecuteAsync(q => q
                    .Where(uf => uf.FriendId == userId)
                    .Select(uf => uf.User)
                    .ToListAsync()));

            return listUser;
        }

        public async Task<bool> AreUsersFriends(int userId, int friendId)
        {
            return await userFriendQueryableRepository
                .ExecuteAsync(q => q.AnyAsync(uf => (uf.UserId == userId && uf.FriendId == friendId) || (uf.UserId == friendId && uf.FriendId == userId)));
        }

        public async Task<Result> RemoveFriend(int userId, int friendUserId)
        {
            var userFriend = await userFriendQueryableRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(uf => uf.UserId == userId && uf.FriendId == friendUserId))
                ??
                await userFriendQueryableRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(uf => uf.UserId == friendUserId && uf.FriendId == userId));

            if (userFriend is null)
                return new Result(false, "Friend not found.");

            await userFriendQueryableRepository.Delete(userFriend);

            await callbackService.NotifyAsync(CallbackType.FriendsListUpdated, CallbackIdGenerator.CreateId(CallbackType.FriendsListUpdated, userId));
            await callbackService.NotifyAsync(CallbackType.FriendsListUpdated, CallbackIdGenerator.CreateId(CallbackType.FriendsListUpdated, friendUserId));
            return new Result(true);
        }
    }
}
