namespace Flycatcher.Services
{
    public class UserStateService
    {
        
        public int? loggedInUserId { get; set; } = null;
        public int? selectedServerId { get; set; } = null;
        public int? selectedChannelId { get; set; } = null;
    }
}
