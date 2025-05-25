using Flycatcher.DataAccess;
using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Services;

namespace Flycatcher.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            return services
                .AddScoped<IQueryableRepository, QueryableRepository>()
                .AddScoped<UserService>()
                .AddScoped<ServerService>()
                .AddScoped<ChannelService>()
                .AddScoped<MessageService>()
                .AddScoped<UserStateService>()
                .AddScoped<SiteAdminService>()
                .AddScoped<ServerInviteService>()

                .AddSingleton<CallbackService>();
        }
    }
}
