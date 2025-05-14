using Microsoft.EntityFrameworkCore;

namespace Flycatcher.Models.Database
{
    [PrimaryKey(nameof(UserId), nameof(FriendId))]
    public class UserFriend
    {
        public int UserId { get; set; }
        public int FriendId { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public User Friend { get; set; } = null!;
    }
}
