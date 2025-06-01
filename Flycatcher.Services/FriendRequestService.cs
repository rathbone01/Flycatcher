using Flycatcher.Models.Results;
using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Models.Database;
using Microsoft.EntityFrameworkCore;
using Flycatcher.Services.Enumerations;

namespace Flycatcher.Services
{
    public class FriendRequestService
    {
        private readonly IQueryableRepository<FriendRequest> friendRequestQueryableRepository;
        private readonly IQueryableRepository<User> userQueryableRepository;
        private readonly CallbackService callbackService;
        private readonly UserService userService;
        private readonly IQueryableRepository<UserFriend> userFriendQueryableRepository;

        public FriendRequestService(IQueryableRepository<FriendRequest> friendRequestQueryableRepository, IQueryableRepository<User> userQueryableRepository, IQueryableRepository<UserFriend> userFriendQueryableRepository, CallbackService callbackService, UserService userService)
        {
            this.friendRequestQueryableRepository = friendRequestQueryableRepository;
            this.userQueryableRepository = userQueryableRepository;
            this.callbackService = callbackService;
            this.userService = userService;
            this.userFriendQueryableRepository = userFriendQueryableRepository;
        }
        public async Task<Result> CreateFriendRequest(int userId, string recipentUserName)
        {
            var recipentUser = await userQueryableRepository
                .GetQueryable()
                .FirstOrDefaultAsync(u => u.Username == recipentUserName);

            if (recipentUser is null)
                return new Result(false, "User not found.");

            if (await userService.AreUsersFriends(userId, recipentUser.Id))
                return new Result(false, "You are already friends.");

            await CreateFriendRequest(userId, recipentUser.Id);
            return new Result(true);
        }

        public async Task<Result> CreateFriendRequest(int userId, int friendId)
        {
            //check there is not a pending request in either direction
            if (friendRequestQueryableRepository.GetQueryable().Any(fr => fr.SenderId == userId && fr.ReceiverId == friendId)
                || friendRequestQueryableRepository.GetQueryable().Any(fr => fr.SenderId == friendId && fr.ReceiverId == userId))
                return new Result { Success = false, ErrorMessage = "Request Already Exists" };

            var friendRequest = new FriendRequest
            {
                SenderId = userId,
                ReceiverId = friendId
            };

            await friendRequestQueryableRepository.Create(friendRequest);
            await callbackService.NotifyAsync(CallbackType.User, friendRequest.ReceiverId);

            return new Result(true);
        }

        public async Task<List<FriendRequest>> GetFriendRequests(int userId)
        {
            return await friendRequestQueryableRepository
                .GetQueryable()
                .Where(fr => fr.ReceiverId == userId)
                .Include(fr => fr.Sender)
                .ToListAsync();
        }

        public async Task<bool> IsFriendRequestPendingToOtherUser(int userId, int otherUserId)
        {
            return await friendRequestQueryableRepository
                .GetQueryable()
                .AnyAsync(fr => fr.SenderId == userId && fr.ReceiverId == otherUserId);
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
    }
}
