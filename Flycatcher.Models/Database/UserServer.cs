using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flycatcher.Models.Database
{
    [PrimaryKey(nameof(UserId), nameof(ServerId))]
    public class UserServer
    {
        public int UserId { get; set; }
        public int ServerId { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Server Server { get; set; } = null!;
    }
}
