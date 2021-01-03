using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silk.Core.Database;

namespace Silk.Core
{
    public static class Startup
    {
        public static IServiceCollection AddDatabase(IServiceCollection services, string connectionString) => 
            services.AddDbContextFactory<SilkDbContext>(
            option =>
            {
                option.UseNpgsql(connectionString);
                #if  DEBUG
                option.EnableSensitiveDataLogging();
                option.EnableDetailedErrors();                                
                #endif // EFCore will complain about enabling sensitive data if you're not in a debug build. //
            }, ServiceLifetime.Transient);
    }
}