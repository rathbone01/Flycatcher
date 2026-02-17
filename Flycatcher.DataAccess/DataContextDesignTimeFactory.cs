using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Flycatcher.DataAccess
{
    // Used only by EF Core tooling (dotnet ef migrations). Not used at runtime.
    public class DataContextDesignTimeFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder.UseSqlServer("Server=localhost;Database=Flycatcher;Trusted_Connection=True;");
            return new DataContext(optionsBuilder.Options);
        }
    }
}
