using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flycatcher.DataAccess.Options
{
    public class ConnectionStringOptions
    {
        public static string SectionName = "ConnectionStrings";
        public required string Flycatcher { get; set; }
    }
}
