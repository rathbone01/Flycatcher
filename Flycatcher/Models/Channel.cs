namespace Flycatcher.Models
{
    public class Channel
    {
        public int Id { get; set; }
        public int ServerId { get; set; }
        public string Name { get; set; } = null!;

        // Navigation properties
        public Server Server { get; set; } = null!;
    }
}
