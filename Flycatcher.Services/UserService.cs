using Flycatcher.Classes;
using Flycatcher.DataAccess;
using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Flycatcher.Models.Results;
using Microsoft.EntityFrameworkCore;

namespace Flycatcher.Services
{
    public class UserService
    {
        private readonly IQueryableRepository queryableRepository;
        private readonly CallbackService callbackService;

        public UserService(IQueryableRepository queryableRepository, CallbackService callbackService)
        {
            this.queryableRepository = queryableRepository;
            this.callbackService = callbackService;
        }

        public string GetUsername(int userId)
        {
            return queryableRepository
                .GetQueryable<User>()
                .FirstOrDefault(u => u.Id == userId)
                ?.Username ?? "Unknown";
        }

        public User? GetUser(int userId)
        {
            return queryableRepository
                .GetQueryable<User>()
                .FirstOrDefault(u => u.Id == userId);
        }

        public User? GetUser(string username)
        {
            return queryableRepository
                .GetQueryable<User>()
                .FirstOrDefault(u => u.Username == username);
        }

        public LoginResult Login(string username, string hashedPassword)
        {
            var user = queryableRepository
                .GetQueryable<User>()
                .FirstOrDefault(u => u.Username == username);

            return Login(user, hashedPassword);
        }

        public LoginResult Login(int userId, string hashedPassword)
        {
            var user = queryableRepository
                .GetQueryable<User>()
                .FirstOrDefault(u => u.Id == userId);

            return Login(user, hashedPassword);
        }

        public LoginResult Login(User? user, string hashedPassword)
        {
            if (user is null)
                return new LoginResult(false, "User not found.");

            if (user.PasswordHash != hashedPassword)
                return new LoginResult(false, "Incorrect password");

            return new LoginResult(user.Id);
        }

        public async Task<Result> CreateUser(string username, string password, string email)
        {
            if (queryableRepository.GetQueryable<User>().Any(u => u.Username == username))
                return new Result(false, "Username already exists.");

            if (queryableRepository.GetQueryable<User>().Any(u => u.Email == email))
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
                Email = email.Trim().ToLower()
            };

            queryableRepository.Create(user);
            await queryableRepository.SaveChangesAsync();

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

        public List<Server> GetUserServers(int userId)
        {
            var servers = queryableRepository
                .GetQueryable<UserServer>()
                .Where(us => us.UserId == userId)
                .Select(us => us.Server)
                .ToList();

            return servers;
        }

        public List<User> GetUserFriends(int userId)
        {
            var listUser = queryableRepository
                .GetQueryable<UserFriend>()
                .Where(uf => uf.UserId == userId)
                .Select(uf => uf.Friend)
                .ToList();

            listUser.AddRange(queryableRepository
                .GetQueryable<UserFriend>()
                .Where(uf => uf.FriendId == userId)
                .Select(uf => uf.User)
                .ToList());

            return listUser;
        }

        public async Task<Result> CreateFriendRequest(int userId, string recipentUserName)
        {
            var recipentUser = queryableRepository
                .GetQueryable<User>()
                .FirstOrDefault(u => u.Username == recipentUserName);

            if (recipentUser is null)
                return new Result(false, "User not found.");

            if (AreUsersFriends(userId, recipentUser.Id))
                return new Result(false, "You are already friends.");

            await CreateFriendRequest(userId, recipentUser.Id);
            return new Result(true);
        }

        public bool AreUsersFriends(int userId, int friendId)
        {
            return queryableRepository
            .GetQueryable<UserFriend>()
            .Any(uf => (uf.UserId == userId && uf.FriendId == friendId) || (uf.UserId == friendId && uf.FriendId == userId));
        }

        public async Task CreateFriendRequest(int userId, int friendId)
        {
            //check there is not a pending request in either direction
            if (queryableRepository.GetQueryable<FriendRequest>().Any(fr => fr.SenderId == userId && fr.ReceiverId == friendId)
                || queryableRepository.GetQueryable<FriendRequest>().Any(fr => fr.SenderId == friendId && fr.ReceiverId == userId))
                return;

            var friendRequest = new FriendRequest
            {
                SenderId = userId,
                ReceiverId = friendId
            };

            queryableRepository.Create(friendRequest);
            await queryableRepository.SaveChangesAsync();
            await callbackService.NotifyAsync(CallbackType.User, friendRequest.ReceiverId);
        }

        public List<FriendRequest> GetFriendRequests(int userId)
        {
            return queryableRepository
                .GetQueryable<FriendRequest>()
                .Where(fr => fr.ReceiverId == userId)
                .Include(fr => fr.Sender)
                .ToList();
        }

        public int GetFriendRequestsCount(int userId)
        {
            return queryableRepository
                .GetQueryable<FriendRequest>()
                .Where(fr => fr.ReceiverId == userId)
                .Include(fr => fr.Sender)
                .Count();
        }

        public async Task AcceptFriendRequest(int friendRequestId)
        {
            var friendRequest = queryableRepository
                .GetQueryable<FriendRequest>()
                .FirstOrDefault(fr => fr.Id == friendRequestId);

            if (friendRequest is null)
                return;

            var userFriend = new UserFriend
            {
                UserId = friendRequest.SenderId,
                FriendId = friendRequest.ReceiverId
            };

            queryableRepository.Create(userFriend);
            queryableRepository.Delete(friendRequest);
            await queryableRepository.SaveChangesAsync();
            await callbackService.NotifyAsync(CallbackType.User, userFriend.UserId);
            await callbackService.NotifyAsync(CallbackType.User, userFriend.FriendId);
        }

        public async Task RejectFriendRequest(int friendRequestId)
        {
            var friendRequest = queryableRepository
                .GetQueryable<FriendRequest>()
                .FirstOrDefault(fr => fr.Id == friendRequestId);

            if (friendRequest is null)
                return;

            var receiverId = friendRequest.ReceiverId;

            queryableRepository.Delete(friendRequest);
            await queryableRepository.SaveChangesAsync();
            await callbackService.NotifyAsync(CallbackType.User, receiverId);
        }

        public async Task<Result> RemoveFriend(int userId, int friendUserId)
        {
            var userFriend = queryableRepository
                .GetQueryable<UserFriend>()
                .FirstOrDefault(uf => uf.UserId == userId && uf.FriendId == friendUserId);

            if (userFriend is null)
            {
                userFriend = queryableRepository
                    .GetQueryable<UserFriend>()
                    .FirstOrDefault(uf => uf.UserId == friendUserId && uf.FriendId == userId);
            }

            if (userFriend is null)
                return new Result(false, "Friend not found.");

            queryableRepository.Delete(userFriend);
            await queryableRepository.SaveChangesAsync();

            await callbackService.NotifyAsync(CallbackType.User, userId);
            await callbackService.NotifyAsync(CallbackType.User, friendUserId);
            return new Result(true);
        }
    }
}
