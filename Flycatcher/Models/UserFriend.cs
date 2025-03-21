using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MudBlazor.CategoryTypes;

namespace Flycatcher.Models
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
