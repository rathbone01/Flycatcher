﻿using Flycatcher.DataAccess;
using Flycatcher.DataAccess.Interfaces;
using Flycatcher.Services;

namespace Flycatcher
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            return services
                .AddScoped<UserService>()
                .AddScoped<ServerService>()
                .AddScoped<ChannelService>()
                .AddScoped<MessageService>()
                .AddScoped<UserStateService>()
                .AddScoped<IQueryableRepository, QueryableRepository>()
                .AddScoped<ServerInviteService>()
                
                .AddSingleton<CallbackService>();
        }
    }
}
