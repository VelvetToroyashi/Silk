using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Extensions.Logging;
using Silk.Core.AutoMod;
using Silk.Core.Commands.General.Tickets;
using Silk.Core.Database;
using Silk.Core.Services;
using Silk.Core.Services.Interfaces;
using Silk.Core.Tools.EventHelpers;
using Silk.Core.Utilities;

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
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<IInfractionService, InfractionService>();
            services.AddSingleton<PrefixCacheService>();
            services.AddSingleton<TicketService>();
            services.AddSingleton<ConfigService>();
            services.AddSingleton<IServiceCacheUpdaterService, ServiceCacheUpdaterService>();

            services.AddSingleton<AutoModInviteHandler>();


            services.AddSingleton<BotExceptionHelper>();
            services.AddSingleton<BotEventSubscriber>();

            services.AddSingleton<GuildAddedHandler>();
            services.AddSingleton<Tools.EventHelpers.MessageAddedHandler>();
            services.AddSingleton<MessageRemovedHandler>();

            services.AddSingleton<MemberRemovedHandler>();

            services.AddSingleton<RoleAddedHandler>();
            services.AddSingleton<RoleRemovedHelper>();

            services.AddSingleton<SerilogLoggerFactory>();

        }

    }
}