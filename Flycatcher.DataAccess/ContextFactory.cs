using Microsoft.Extensions.Options;
using Flycatcher.DataAccess.Options;
using Microsoft.EntityFrameworkCore;

namespace Flycatcher.DataAccess
{
    public class ContextFactory
    {
        private readonly IOptions<ConnectionStringOptions> connectionStringOptions;
        public ContextFactory(IOptions<ConnectionStringOptions> connectionStringOptions)
        {
            this.connectionStringOptions = connectionStringOptions;
        }

        public DataContext CreateDbContext()
        {
            var connectionString = connectionStringOptions.Value.Flycatcher;
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>().UseSqlServer(connectionString);
            return new DataContext(optionsBuilder.Options);
        }
    }
}
