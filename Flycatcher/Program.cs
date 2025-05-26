using Flycatcher.DataAccess;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;
using Flycatcher.Components;
using Flycatcher.Configuration;
using Flycatcher.DataAccess.Options;

namespace Flycatcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddMudServices();

            var logLocation = Path.Combine(builder.Environment.ContentRootPath, "logs", "log-.txt");
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(logLocation, rollingInterval: RollingInterval.Day)
                .CreateLogger();
            builder.Logging.AddSerilog(logger);

            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            var cfg = new ConfigurationBuilder()
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.Configure<ConnectionStringOptions>(cfg.GetSection("ConnectionStrings"));
            builder.Services.AddApplicationServices();

            StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var contextFactory = scope.ServiceProvider.GetRequiredService<ContextFactory>();
                var db = contextFactory.CreateDbContext();
                db.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
