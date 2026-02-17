using Flycatcher.DataAccess;
using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Services;

namespace Flycatcher.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, ConfigurationSection connectionStrings)
        {
            var connectionString = connectionStrings.GetConnectionString("Flycatcher");

            services.AddDbContextFactory<DataContext>();
            services.AddScoped(typeof(IQueryableRepository<>), typeof(QueryableRepository<>));
            services.AddScoped<UserService>();
            services.AddScoped<ServerService>();
            services.AddScoped<ChannelService>();
            services.AddScoped<MessageService>();
            services.AddScoped<UserStateService>();
            services.AddScoped<SiteAdminService>();
            services.AddScoped<ServerInviteService>();
            services.AddScoped<FriendRequestService>();
            services.AddSingleton<CallbackService>();

            return services;
        }
    }
}
