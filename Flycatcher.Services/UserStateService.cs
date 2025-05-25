namespace Flycatcher.Services
{
    public class UserStateService
    {
        public string? loggedInUsername { get; set; } = null;
        public int? loggedInUserId { get; set; } = null;
        public int? selectedServerId { get; set; } = null;
        public int? selectedChannelId { get; set; } = null;
        public int? selectedFriendChat { get; set; } = null;
    }
}
