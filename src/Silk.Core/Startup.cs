using System;
using System.Runtime.CompilerServices;
using MediatR;
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
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void AddDatabase(IServiceCollection services, string connectionString)
        {
            services.AddDbContextFactory<SilkDbContext>(
                option =>
                {
                    option.UseNpgsql(connectionString);
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
            services.AddTransient<SilkDbContext>();
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
            services.AddScoped<IInfractionService, InfractionService>();
            services.AddTransient<IPrefixCacheService, PrefixCacheService>();
            services.AddSingleton<IServiceCacheUpdaterService, ServiceCacheUpdaterService>();

            services.AddSingleton<TagService>();
            
            services.AddHostedService<Bot>();

            services.AddMediatR(typeof(Program));
            services.AddMediatR(typeof(SilkDbContext));
        }
    }
}