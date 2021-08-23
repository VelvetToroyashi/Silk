using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silk.Core.Data;

namespace Silk.Dashboard
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await EnsureDatabaseCreatedAndApplyMigrations(host);
            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddJsonFile("appSettings.json", true, false);
                    configuration.AddUserSecrets<Startup>(true, false);
                })
                .ConfigureWebHostDefaults(webHostBuilder => { webHostBuilder.UseStartup<Startup>(); });

        private static async Task EnsureDatabaseCreatedAndApplyMigrations(IHost builtBuilder)
        {
            try
            {
                using IServiceScope? serviceScope = builtBuilder.Services?.CreateScope();
                if (serviceScope is not null)
                {
                    await using GuildContext dbContext = serviceScope.ServiceProvider
                        .GetRequiredService<IDbContextFactory<GuildContext>>()
                        .CreateDbContext();

                    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

                    if (pendingMigrations.Any())
                        await dbContext.Database.MigrateAsync();
                }
            }
            catch (Exception)
            {
                /* Ignored. Todo: Probably should handle? */
            }
        }
    }
}