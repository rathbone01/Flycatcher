using Flycatcher.Classes;
using Flycatcher.Models.Database;
using Flycatcher.Models.Results;
using Flycatcher.Repositories;

namespace Flycatcher.Services
{
    public class UserService
    {
        public int? LoggedInUserId { get; set; } = null;
        public string? LoggedInUsername { get; set; } = null;
        public DateTime? LoggedInTime { get; set; }
        public bool IsLoggedIn => LoggedInUserId.HasValue;

        private readonly QueryableRepository queryableRepository;

        public UserService(QueryableRepository queryableRepository)
        {
            this.queryableRepository = queryableRepository;
        }

        public Result Login(int userId, string hashedPassword)
        {
            var user = queryableRepository
                .GetQueryable<User>()
                .FirstOrDefault(u => u.Id == userId);

            if (user is null)
                return new Result(false, "User not found.");

            if (user.PasswordHash != hashedPassword)
                return new Result(false, "Incorrect password");

            LoggedInUserId = userId;
            LoggedInTime = DateTime.UtcNow;
            LoggedInUsername = user.Username;

            return new Result(true);
        }

        public void Logout()
        {
            LoggedInUserId = null;
            LoggedInTime = null;
            LoggedInUsername = null;
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
            await queryableRepository.SaveChanges();

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

        public List<Server> GetUserServers()
        {
            return queryableRepository
                .GetQueryable<UserServer>()
                .Where(us => us.UserId == LoggedInUserId)
                .Select(us => us.Server)
                .ToList();
        }

        public List<User> GetUserFriends()
        {
            var listUser = queryableRepository
                .GetQueryable<UserFriend>()
                .Where(uf => uf.UserId == LoggedInUserId)
                .Select(uf => uf.Friend)
                .ToList();

            listUser.AddRange(queryableRepository
                .GetQueryable<UserFriend>()
                .Where(uf => uf.FriendId == LoggedInUserId)
                .Select(uf => uf.User)
                .ToList());

            return listUser;
        }

        public async Task CreateFriendRequest(int userId, string recipentUserName)
        {
            var recipentUser = queryableRepository
                .GetQueryable<User>()
                .FirstOrDefault(u => u.Username == recipentUserName);

            if (recipentUser is null)
                return;

            await CreateFriendRequest(userId, recipentUser.Id);
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
            await queryableRepository.SaveChanges();
        }

        public List<FriendRequest> GetFriendRequests(int userId)
        {
            return queryableRepository
                .GetQueryable<FriendRequest>()
                .Where(fr => fr.ReceiverId == userId)
                .ToList();
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
            await queryableRepository.SaveChanges();
        }

        public async Task RejectFriendRequest(int friendRequestId)
        {
            var friendRequest = queryableRepository
                .GetQueryable<FriendRequest>()
                .FirstOrDefault(fr => fr.Id == friendRequestId);

            if (friendRequest is null)
                return;

            queryableRepository.Delete(friendRequest);
            await queryableRepository.SaveChanges();
        }
    }
}
