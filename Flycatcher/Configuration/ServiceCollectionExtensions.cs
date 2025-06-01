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
                .AddScoped<ContextFactory>()
                .AddScoped(typeof(IQueryableRepository<>), typeof(QueryableRepository<>))
                .AddScoped<UserService>()
                .AddScoped<ServerService>()
                .AddScoped<ChannelService>()
                .AddScoped<MessageService>()
                .AddScoped<UserStateService>()
                .AddScoped<SiteAdminService>()
                .AddScoped<ServerInviteService>()
                .AddScoped<FriendRequestService>()

                .AddSingleton<CallbackService>();
        }
    }
}
