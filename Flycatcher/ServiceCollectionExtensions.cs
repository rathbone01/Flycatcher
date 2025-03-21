using Flycatcher.Repositories;
using Flycatcher.Services;

namespace Flycatcher
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            return services
                .AddScoped<UserService>()
                .AddScoped<QueryableRepository>();
        }
    }
}
