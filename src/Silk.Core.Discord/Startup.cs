using System.Runtime.CompilerServices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Extensions.Logging;
using Silk.Core.Data;
using Silk.Core.Discord.Commands.General.Tickets;
using Silk.Core.Discord.EventHandlers;
using Silk.Core.Discord.EventHandlers.MemberAdded;
using Silk.Core.Discord.EventHandlers.MessageAdded;
using Silk.Core.Discord.EventHandlers.MessageAdded.AutoMod;
using Silk.Core.Discord.Services;
using Silk.Core.Discord.Services.Interfaces;
using Silk.Core.Discord.Utilities.Bot;

namespace Silk.Core.Discord
{
    public static class Startup
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void AddDatabase(IServiceCollection services, string connectionString)
        {
            services.AddDbContextFactory<GuildContext>(
                option =>
                {
                    NpgsqlDbContextOptionsBuilderExtensions.UseNpgsql(option, connectionString);
                    #if DEBUG
                    option.EnableSensitiveDataLogging();
                    option.EnableDetailedErrors();
                    #endif // EFCore will complain about enabling sensitive data if you're not in a debug build. //
                }, ServiceLifetime.Transient);
        }



        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void AddServices(IServiceCollection services)
        {
            services.AddTransient<ConfigService>();
            services.AddTransient<GuildContext>();
            services.AddTransient<TicketService>();
            services.AddSingleton<AntiInviteCore>();
            services.AddTransient<RoleAddedHandler>();
            services.AddTransient<GuildAddedHandler>();
            services.AddTransient<MemberAddedHandler>();
            services.AddTransient<RoleRemovedHandler>();
            services.AddSingleton<BotExceptionHandler>();
            services.AddSingleton<SerilogLoggerFactory>();
            services.AddTransient<MessageCreatedHandler>();
            services.AddTransient<MessageRemovedHandler>();

            services.AddScoped<IInputService, InputService>();

            services.AddScoped<IInfractionService, InfractionService>();
            services.AddTransient<IPrefixCacheService, PrefixCacheService>();
            services.AddSingleton<IServiceCacheUpdaterService, ServiceCacheUpdaterService>();

            services.AddSingleton<TagService>();

            services.AddHostedService<Bot>();
            services.AddHostedService<StatusService>();

            //Copped this hack from: https://stackoverflow.com/a/65552373 //
            services.AddSingleton<ReminderService>();
            services.AddHostedService(b => b.GetRequiredService<ReminderService>());

            services.AddMediatR(typeof(Program));
            services.AddMediatR(typeof(GuildContext));
        }
    }
}