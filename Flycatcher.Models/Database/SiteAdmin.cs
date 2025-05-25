using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flycatcher.Models.Database
{
    public class SiteAdmin
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
