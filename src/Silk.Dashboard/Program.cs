using Microsoft.EntityFrameworkCore;
using Silk.Data;

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