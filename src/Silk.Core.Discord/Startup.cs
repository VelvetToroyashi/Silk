using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Extensions.Logging;
using Silk.Core.Data;
using Silk.Core.Discord.EventHandlers;
using Silk.Core.Discord.EventHandlers.MemberAdded;
using Silk.Core.Discord.EventHandlers.MessageAdded.AutoMod;
using Silk.Core.Discord.Services;
using Silk.Core.Discord.Services.Interfaces;
using Silk.Core.Discord.Utilities.Bot;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Core.Discord
{
    public class Startup
    {
        public static void AddServices(IServiceCollection services)
        {
            services.AddTransient<ConfigService>();
            services.AddTransient<GuildContext>();
            services.AddSingleton<AntiInviteCore>();
            services.AddTransient<RoleAddedHandler>();
            services.AddTransient<GuildAddedHandler>();
            services.AddTransient<MemberAddedHandler>();
            services.AddTransient<RoleRemovedHandler>();
            services.AddSingleton<BotExceptionHandler>();
            services.AddSingleton<SerilogLoggerFactory>();
            services.AddTransient<MessageRemovedHandler>();

            services.AddScoped<IInputService, InputService>();

            services.AddScoped<IInfractionService, InfractionService>();
            services.AddTransient<IPrefixCacheService, PrefixCacheService>();
            services.AddSingleton<IServiceCacheUpdaterService, ServiceCacheUpdaterService>();

            services.AddSingleton<TagService>();



            services.AddSingleton<IMessageSender, MessageSenderService>();

            //Copped this hack from: https://stackoverflow.com/a/65552373 //
            services.AddSingleton<ReminderService>();

            services.AddHostedService(b => b.GetRequiredService<ReminderService>());
            services.AddHostedService<StatusService>();
            services.AddHostedService<Bot>();



            services.AddMediatR(typeof(Program));
            services.AddMediatR(typeof(GuildContext));
        }
    }
}