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
        private readonly CallbackService callbackService;

        public UserService(IQueryableRepository<User> userQueryableRepository, IQueryableRepository<SiteAdmin> siteAdminQueryableRepository, IQueryableRepository<UserServer> userServerQueryableRepository, IQueryableRepository<UserFriend> userFriendQueryableRepository, IQueryableRepository<FriendRequest> friendRequestQueryableRepository, CallbackService callbackService)
        {
            this.userQueryableRepository = userQueryableRepository;
            this.callbackService = callbackService;
            this.siteAdminQueryableRepository = siteAdminQueryableRepository;
            this.userServerQueryableRepository = userServerQueryableRepository;
            this.userFriendQueryableRepository = userFriendQueryableRepository;
            this.friendRequestQueryableRepository = friendRequestQueryableRepository;
        }

        public async Task<string> GetUsername(int userId)
        {
            var user = await userQueryableRepository
                 .GetQueryable()
                 .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
                return string.Empty;

            return user.Username;
        }

        public User? GetUser(int userId)
        {
            return userQueryableRepository
                .GetQueryable()
                .FirstOrDefault(u => u.Id == userId);
        }

        public async Task<User?> GetUser(string username)
        {
            return await userQueryableRepository
                .GetQueryable()
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> IsUserSiteAdmin(int userId)
        {
            var siteAdmin = await siteAdminQueryableRepository
                .GetQueryable()
                .FirstOrDefaultAsync(sa => sa.UserId == userId);

            return siteAdmin != null;
        }

        public async Task<LoginResult> Login(string username, string hashedPassword)
        {
            var user = await userQueryableRepository
                .GetQueryable()
                .FirstOrDefaultAsync(u => u.Username == username);

            return Login(user, hashedPassword);
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
            if (await userQueryableRepository.GetQueryable().AnyAsync(u => u.Username == username))
                return new Result(false, "Username already exists.");

            if (await userQueryableRepository.GetQueryable().AnyAsync(u => u.Email == email))
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
            var servers = await userServerQueryableRepository
                .GetQueryable()
                .Where(us => us.UserId == userId)
                .Select(us => us.Server)
                .ToListAsync();

            return servers;
        }

        public async Task<List<User>> GetUserFriends(int userId)
        {
            var listUser = await userFriendQueryableRepository
                .GetQueryable()
                .Where(uf => uf.UserId == userId)
                .Select(uf => uf.Friend)
                .ToListAsync();

            listUser.AddRange(await userFriendQueryableRepository
                .GetQueryable()
                .Where(uf => uf.FriendId == userId)
                .Select(uf => uf.User)
                .ToListAsync());

            return listUser;
        }

        public async Task<Result> CreateFriendRequest(int userId, string recipentUserName)
        {
            var recipentUser = await userQueryableRepository
                .GetQueryable()
                .FirstOrDefaultAsync(u => u.Username == recipentUserName);

            if (recipentUser is null)
                return new Result(false, "User not found.");

            if (await AreUsersFriends(userId, recipentUser.Id))
                return new Result(false, "You are already friends.");

            await CreateFriendRequest(userId, recipentUser.Id);
            return new Result(true);
        }

        public async Task<bool> AreUsersFriends(int userId, int friendId)
        {
            return await userFriendQueryableRepository
            .GetQueryable()
            .AnyAsync(uf => (uf.UserId == userId && uf.FriendId == friendId) || (uf.UserId == friendId && uf.FriendId == userId));
        }

        public async Task CreateFriendRequest(int userId, int friendId)
        {
            //check there is not a pending request in either direction
            if (friendRequestQueryableRepository.GetQueryable().Any(fr => fr.SenderId == userId && fr.ReceiverId == friendId)
                || friendRequestQueryableRepository.GetQueryable().Any(fr => fr.SenderId == friendId && fr.ReceiverId == userId))
                return;

            var friendRequest = new FriendRequest
            {
                SenderId = userId,
                ReceiverId = friendId
            };

            await friendRequestQueryableRepository.Create(friendRequest);
            await callbackService.NotifyAsync(CallbackType.User, friendRequest.ReceiverId);
        }

        public async Task<List<FriendRequest>> GetFriendRequests(int userId)
        {
            return await friendRequestQueryableRepository
                .GetQueryable()
                .Where(fr => fr.ReceiverId == userId)
                .Include(fr => fr.Sender)
                .ToListAsync();
        }

        public int GetFriendRequestsCount(int userId)
        {
            return friendRequestQueryableRepository
                .GetQueryable()
                .Where(fr => fr.ReceiverId == userId)
                .Include(fr => fr.Sender)
                .Count();
        }

        public async Task AcceptFriendRequest(int friendRequestId)
        {
            var friendRequest = await friendRequestQueryableRepository
                .GetQueryable()
                .FirstOrDefaultAsync(fr => fr.Id == friendRequestId);

            if (friendRequest is null)
                return;

            var userFriend = new UserFriend
            {
                UserId = friendRequest.SenderId,
                FriendId = friendRequest.ReceiverId
            };

            await userFriendQueryableRepository.Create(userFriend);
            await friendRequestQueryableRepository.Delete(friendRequest);

            await callbackService.NotifyAsync(CallbackType.User, userFriend.UserId);
            await callbackService.NotifyAsync(CallbackType.User, userFriend.FriendId);
        }

        public async Task RejectFriendRequest(int friendRequestId)
        {
            var friendRequest = await friendRequestQueryableRepository
                .GetQueryable()
                .FirstOrDefaultAsync(fr => fr.Id == friendRequestId);

            if (friendRequest is null)
                return;

            var receiverId = friendRequest.ReceiverId;

            await friendRequestQueryableRepository.Delete(friendRequest);
            await callbackService.NotifyAsync(CallbackType.User, receiverId);
        }

        public async Task<Result> RemoveFriend(int userId, int friendUserId)
        {
            var userFriend = await userFriendQueryableRepository
                .GetQueryable()
                .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.FriendId == friendUserId)
                ?? 
                await userFriendQueryableRepository
                .GetQueryable()
                .FirstOrDefaultAsync(uf => uf.UserId == friendUserId && uf.FriendId == userId);

            if (userFriend is null)
                return new Result(false, "Friend not found.");

            await userFriendQueryableRepository.Delete(userFriend);

            await callbackService.NotifyAsync(CallbackType.User, userId);
            await callbackService.NotifyAsync(CallbackType.User, friendUserId);
            return new Result(true);
        }


    }
}
