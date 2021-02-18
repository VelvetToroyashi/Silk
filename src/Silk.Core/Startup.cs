using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Extensions.Logging;
using Silk.Core.Commands.General.Tickets;
using Silk.Core.EventHandlers;
using Silk.Core.EventHandlers.MemberAdded;
using Silk.Core.EventHandlers.MessageAdded;
using Silk.Core.EventHandlers.MessageAdded.AutoMod;
using Silk.Core.Services;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities.Bot;
using Silk.Data;

namespace Silk.Core
{
    public static class Startup
    {
        public static IServiceCollection AddDatabase(IServiceCollection services, string connectionString) =>
            services.AddDbContextFactory<SilkDbContext>(
                option =>
                {
                    option.UseNpgsql(connectionString);
                    #if DEBUG
                    option.EnableSensitiveDataLogging();
                    option.EnableDetailedErrors();
                    #endif // EFCore will complain about enabling sensitive data if you're not in a debug build. //
                }, ServiceLifetime.Transient);

        public static void AddServices(IServiceCollection services)
        {
            services.AddScoped<SilkDbContext>();
            services.AddScoped<IDatabaseService, DatabaseService>();
            services.AddTransient<IInfractionService, InfractionService>();
            services.AddTransient<IPrefixCacheService, PrefixCacheService>();
            services.AddTransient<TicketService>();
            services.AddTransient<ConfigService>();
            services.AddSingleton<IServiceCacheUpdaterService, ServiceCacheUpdaterService>();

            services.AddTransient<AutoModInviteHandler>();

            services.AddSingleton<BotExceptionHandler>();

            services.AddTransient<GuildAddedHandler>();
            services.AddTransient<MessageAddedHandler>();
            services.AddTransient<MessageRemovedHandler>();

            services.AddTransient<MemberAddedHandler>();
            services.AddTransient<MemberRemovedHandler>();

            services.AddTransient<RoleAddedHandler>();
            services.AddTransient<RoleRemovedHandler>();

            services.AddSingleton<SerilogLoggerFactory>();

        }

    }
}