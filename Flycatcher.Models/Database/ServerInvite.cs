namespace Flycatcher.Models.Database
{
    public class ServerInvite
    {
        public int Id { get; set; }
        public int ServerId { get; set; }
        public int SenderUserId { get; set; }
        public int RecieverUserId { get; set; }

        // Navigation properties
        public Server Server { get; set; } = null!;
        public User SenderUser { get; set; } = null!;
        public User RecieverUser { get; set; } = null!;
    }
}
 