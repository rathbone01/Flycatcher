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
                .ExecuteAsync(q => q.FirstOrDefaultAsync(u => u.Username == recipentUserName));

            if (recipentUser is null)
                return new Result(false, "User not found.");

            if (await userService.AreUsersFriends(userId, recipentUser.Id))
                return new Result(false, "You are already friends.");

            await CreateFriendRequest(userId, recipentUser.Id);
            return new Result(true);
        }

        public async Task<Result> CreateFriendRequest(int userId, int friendId)
        {
            //check there is not a pending request in this direction
            if (await IsFriendRequestPendingToOtherUser(userId, friendId))
                return new Result { Success = false, ErrorMessage = "Request Already Exists" };

            //check there is not a pending request in the other direction, if there is, accept it
            if (await IsFriendRequestPendingToOtherUser(friendId, userId))
            {
                await AcceptFriendRequest(friendId);
                return new Result(true);
            }

            var friendRequest = new FriendRequest
            {
                SenderId = userId,
                ReceiverId = friendId
            };

            await friendRequestQueryableRepository.Create(friendRequest);
            await callbackService.NotifyAsync(CallbackType.FriendRequest, CallbackIdGenerator.CreateId(CallbackType.FriendRequest, friendRequest.ReceiverId));

            return new Result(true);
        }

        public async Task<List<FriendRequest>> GetFriendRequests(int userId)
        {
            return await friendRequestQueryableRepository
                .ExecuteAsync(q => q
                    .Where(fr => fr.ReceiverId == userId)
                    .Include(fr => fr.Sender)
                    .ToListAsync());
        }

        public async Task<bool> IsFriendRequestPendingToOtherUser(int userId, int otherUserId)
        {
            return await friendRequestQueryableRepository
                .ExecuteAsync(q => q.AnyAsync(fr => fr.SenderId == userId && fr.ReceiverId == otherUserId));
        }

        public async Task<int> GetFriendRequestsCount(int userId)
        {
            return await friendRequestQueryableRepository
                .ExecuteAsync(q => q
                    .Where(fr => fr.ReceiverId == userId)
                    .Include(fr => fr.Sender)
                    .CountAsync());
        }

        public async Task AcceptFriendRequest(int friendRequestId)
        {
            var friendRequest = await friendRequestQueryableRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(fr => fr.Id == friendRequestId));

            if (friendRequest is null)
                return;

            var userFriend = new UserFriend
            {
                UserId = friendRequest.SenderId,
                FriendId = friendRequest.ReceiverId
            };

            await userFriendQueryableRepository.Create(userFriend);
            await friendRequestQueryableRepository.Delete(friendRequest);

            await callbackService.NotifyAsync(CallbackType.FriendsListUpdated, CallbackIdGenerator.CreateId(CallbackType.FriendsListUpdated, userFriend.UserId));
            await callbackService.NotifyAsync(CallbackType.FriendsListUpdated, CallbackIdGenerator.CreateId(CallbackType.FriendsListUpdated, userFriend.FriendId));
        }

        public async Task RejectFriendRequest(int friendRequestId)
        {
            var friendRequest = await friendRequestQueryableRepository
                .ExecuteAsync(q => q.FirstOrDefaultAsync(fr => fr.Id == friendRequestId));

            if (friendRequest is null)
                return;

            var receiverId = friendRequest.ReceiverId;

            await friendRequestQueryableRepository.Delete(friendRequest);
            await callbackService.NotifyAsync(CallbackType.FriendRequest, CallbackIdGenerator.CreateId(CallbackType.FriendRequest, receiverId));
        }
    }
}
