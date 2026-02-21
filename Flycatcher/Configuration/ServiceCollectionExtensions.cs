using Flycatcher.DataAccess;
using Flycatcher.DataAccess.Interfaces;
using Flycatcher.DataAccess.Options;
using Flycatcher.Services;
using Microsoft.EntityFrameworkCore;

namespace Flycatcher.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, ConnectionStringOptions connectionStringOptions)
        {
            services.AddDbContextFactory<DataContext>(options => options.UseSqlServer(connectionStringOptions.Flycatcher));
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
            services.AddScoped<DirectMessageService>();
            services.AddScoped<RoleService>();
            services.AddScoped<UserRoleService>();
            services.AddScoped<PermissionService>();
            services.AddScoped<ChannelPermissionService>();
            services.AddScoped<UserReportService>();
            services.AddScoped<UserBanService>();
            services.AddScoped<UserTimeoutService>();
            services.AddScoped<MessageValidationService>();
            services.AddScoped<BrowserTimeService>();

            return services;
        }
    }
}
